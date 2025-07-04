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

});

function validateForm(formData) {
    // 取得民國年欄位
    const strApplyStart = document.getElementById('applyStartDate').value.trim();
    const strApplyEnd = document.getElementById('applyEndDate').value.trim();
    const strCompletionStart = document.getElementById('completionStartDate').value.trim();
    const strCompletionEnd = document.getElementById('completionEndDate').value.trim();

    // 驗證格式
    const dateFields = [
        { id: 'applyStartDate', value: strApplyStart },
        { id: 'applyEndDate', value: strApplyEnd },
        { id: 'completionStartDate', value: strCompletionStart },
        { id: 'completionEndDate', value: strCompletionEnd }
    ];

    for (const field of dateFields) {
        if (field.value && !isValidRocDate(field.value)) {
            alert('請輸入有效的日期（如1130701）');
            document.getElementById(field.id).focus();
            return false;
        }
    }

    // 申報日期區間檢查
    if (strApplyStart && strApplyEnd) {
        if (parseInt(strApplyStart, 10) > parseInt(strApplyEnd, 10)) {
            alert('申報起始日不能晚於結束日');
            document.getElementById('applyStartDate').focus();
            return false;
        }
    }

    // 完成日期區間檢查
    if (strCompletionStart && strCompletionEnd) {
        if (parseInt(strCompletionStart, 10) > parseInt(strCompletionEnd, 10)) {
            alert('完成起始日不能晚於結束日');
            document.getElementById('completionStartDate').focus();
            return false;
        }
    }

    // 轉換成西元年並暫存到隱藏欄位或 FormData
    if (strApplyStart) formData.set('applyStartDate', rocToAD(strApplyStart));
    if (strApplyEnd) formData.set('applyEndDate', rocToAD(strApplyEnd));
    if (strCompletionStart) formData.set('completionStartDate', rocToAD(strCompletionStart));
    if (strCompletionEnd) formData.set('completionEndDate', rocToAD(strCompletionEnd));

    return true;
}


const pageSize = 10; // 每頁顯示的記錄數
//搜尋並渲染
async function searchAndRender(formData, gotoPage = 1) {
    try {

        formData.set('page', gotoPage);
        formData.set('pageSize', pageSize);

        const res = await fetch('/Home/Search', {
            method: 'POST',
            body: formData
        });

        if (!res.ok) throw new Error('查詢失敗');

        const response = await res.json();
        if (!response.success) throw new Error(response.message);

        let html = '';
        let data = response.data.results || [];
        let totalCount = response.data.totalCount || 0;

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

        renderPagination(gotoPage, totalCount);

    } catch (err) {
        document.getElementById('searchResults').innerHTML = '<div class="text-danger">查詢失敗：' + err + '</div>';
    }
}

function renderPagination(page, totalCount) {
    const totalPages = Math.ceil(totalCount / pageSize);
    let html = '';

    if (totalPages <= 1) {
        $('#pagination').html('');
        return;
    }

    // 上一頁
    html += `<li class="page-item${page === 1 ? ' disabled' : ''}">
        <a class="page-link" href="#" data-page="${page - 1}">«</a>
    </li>`;

    // 計算分頁範圍
    let start = Math.max(1, page - 2);
    let end = Math.min(totalPages, page + 2);
    if (end - start < 4) {
        if (start === 1) {
            end = Math.min(totalPages, start + 4);
        } else if (end === totalPages) {
            start = Math.max(1, end - 4);
        }
    }

    if (start > 1) {
        html += `<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>`;
        if (start > 2) html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
    }

    for (let i = start; i <= end; i++) {
        html += `<li class="page-item${i === page ? ' active' : ''}">
            <a class="page-link" href="#" data-page="${i}">${i}</a>
        </li>`;
    }

    if (end < totalPages) {
        if (end < totalPages - 1) html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
        html += `<li class="page-item"><a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a></li>`;
    }

    // 下一頁
    html += `<li class="page-item${page === totalPages ? ' disabled' : ''}">
        <a class="page-link" href="#" data-page="${page + 1}">»</a>
    </li>`;

    $('#pagination').html(html);

    // 綁定點擊事件
    $('#pagination .page-link').off('click').on('click',async function (e) {
        e.preventDefault();
        const gotoPage = parseInt($(this).data('page'));
        if (!isNaN(gotoPage) && gotoPage >= 1 && gotoPage <= totalPages && gotoPage !== page) {
            var searchForm = document.getElementById('searchForm');
            const formData = new FormData(searchForm);
            if (!validateForm(formData)) return;
            await searchAndRender(formData, gotoPage);
        }
    });
}
