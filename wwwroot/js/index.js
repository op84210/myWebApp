document.addEventListener('DOMContentLoaded', async function () {

    //搜尋事件
    var searchForm = document.getElementById('searchForm');
    if (searchForm) {

        searchForm.addEventListener('submit', async function (e) {
            e.preventDefault();

            const formData = new FormData(searchForm);
            if (!validateForm(formData)) return;

            //搜尋並渲染
            await searchAndRender(formData);

            // 渲染表格後，監聽所有編輯按鈕
            document.getElementById('searchResults').addEventListener('click', function (e) {
                if (e.target && e.target.classList.contains('edit_btn')) {
                    const record_id = e.target.getAttribute('data-id');
                    if (record_id) {
                        window.location.href = `/Home/Edit/${record_id}`;
                    }
                }
            });
        });
    }

    // 新增按鈕事件
    var addBtn = document.getElementById('addBtn');
    if (addBtn) {
        addBtn.addEventListener('click', function () {
            window.location.href = '/Home/Create';
        });
    }

    //依據選取的單位列出人員
    document.getElementById('depart_code').addEventListener('change', async function () {
        await getStaffsByDepartment(this.value, 'processing_staff_id');
    });

});

function validateForm(formData) {

    // 取得民國年欄位
    const applyStart = document.getElementById('applyStartDate').value.trim();
    const applyEnd = document.getElementById('applyEndDate').value.trim();
    const completionStart = document.getElementById('completionStartDate').value.trim();
    const completionEnd = document.getElementById('completionEndDate').value.trim();

    // 驗證格式
    const dateFields = [
        { id: 'applyStartDate', value: applyStart },
        { id: 'applyEndDate', value: applyEnd },
        { id: 'completionStartDate', value: completionStart },
        { id: 'completionEndDate', value: completionEnd }
    ];

    for (const field of dateFields) {
        if (field.value && !/^\d{7}$/.test(field.value)) {
            alert('請輸入正確的民國年格式（如1130701）');
            document.getElementById(field.id).focus();
            return false;
        }
    }

    // 轉換成西元年並暫存到隱藏欄位或 FormData
    if (applyStart) formData.set('applyStartDate', rocToAD(applyStart));
    if (applyEnd) formData.set('applyEndDate', rocToAD(applyEnd));
    if (completionStart) formData.set('completionStartDate', rocToAD(completionStart));
    if (completionEnd) formData.set('completionEndDate', rocToAD(completionEnd));

    return true;
}

//搜尋並渲染
async function searchAndRender(formData) {
    try {
        const res = await fetch('/Home/Search', {
            method: 'POST',
            body: formData
        });

        if (!res.ok) throw new Error('查詢失敗');

        const response = await res.json();
        if (!response.success) throw new Error(response.message);

        let html = '';
        let data = response.data || [];

        if (data.length === 0) {
            html = '<div>查無資料</div>';
        } else {
            html = data.map(row => `
                    <tr>
                        <td>${row.record_id || ''}</td>
                        <td>${toTaiwanDate(row.apply_date, "R/MM/dd HH:mm") || ''}</td>
                        <td>${row.depart_name || ''}</td>
                        <td>${row.staff_name || ''}</td>
                        <td>${row.problem_type || ''}</td>
                        <td>${row.processing_type || ''}</td>
                        <td>${row.processing_staff_name || ''}</td>
                        <td>${toTaiwanDate(row.completion_date, "R/MM/dd HH:mm") || ''}</td>
                        <td><button class="btn btn-secondary edit_btn" data-id="${row.record_id}">編輯</button></td>
                    </tr>
                `).join('');
            html = '<table class="table table-bordered"><thead><tr>' +
                '<th>序號</th><th>申報日期</th><th>使用單位</th><th>使用者</th><th>問題類別</th><th>處理類別</th><th>處理人員</th><th>完成日期</th><th>編輯</th>' +
                '</tr></thead><tbody>' + html + '</tbody></table>';
        }

        document.getElementById('searchResults').innerHTML = html;


    } catch (err) {
        document.getElementById('searchResults').innerHTML = '<div class="text-danger">查詢失敗：' + err + '</div>';
    }
}
