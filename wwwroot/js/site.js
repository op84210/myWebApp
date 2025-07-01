//西元年轉換民國年
function toTaiwanDate(isoDateStr, format = "R/MM/dd HH:mm") {
    if (!isoDateStr) return "";
    const date = new Date(isoDateStr);
    if (isNaN(date)) return isoDateStr;
    const year = date.getFullYear() - 1911;
    const month = (date.getMonth() + 1).toString().padStart(2, "0");
    const day = date.getDate().toString().padStart(2, "0");
    const hour = date.getHours().toString().padStart(2, "0");
    const min = date.getMinutes().toString().padStart(2, "0");
    return format
        .replace("R", year)
        .replace("MM", month)
        .replace("dd", day)
        .replace("HH", hour)
        .replace("mm", min);
}

//民國年轉換西元年
function rocToAD(rocStr) {
    // 1130701 → 2024-07-01
    if (!rocStr) return "";
    const m = rocStr.match(/^(\d{3})(\d{2})(\d{2})$/);
    if (!m) return "";
    const year = parseInt(m[1], 10) + 1911;
    const month = m[2];
    const day = m[3];
    return `${year}-${month}-${day}`;
}

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

// 下拉選單初始化
async function initDropdowns() {

    try {
        const res = await fetch('/Home/GetDropdownOptions');
        if (!res.ok) throw new Error('伺服器錯誤');
        const data = await res.json();

        const dropdownMap = [
            { id: 'departCode', dataKey: 'departments' }, 
            { id: 'problemType', dataKey: 'problemTypes' },
            { id: 'processingStaffId', dataKey: 'processingStaffIds' },
            { id: 'processingType', dataKey: 'processingTypes' }
        ];

        dropdownMap.forEach(item => {
            const select = document.getElementById(item.id);
            data[item.dataKey].forEach(opt => {
                const newOpt = document.createElement('option');
                newOpt.value = opt.code;
                newOpt.textContent = opt.name;
                select.appendChild(newOpt);            
            });
        });

    } catch (err) {
        alert('下拉選單載入失敗：' + err);
    }
}

//搜尋並渲染
async function searchAndRender(formData) {
    try {
        const res = await fetch('/Home/Search', {
            method: 'POST',
            body: formData
        });
        if (!res.ok) throw new Error('查詢失敗');
        const data = await res.json();

        let html = '';
        if (data.length === 0) {
            html = '<div>查無資料</div>';
        } else {
            html = data.map(row => `
                <tr>
                    <td>${row.RecordId || ''}</td>
                    <td>${toTaiwanDate(row.ApplyDate, "R/MM/dd HH:mm") || ''}</td>
                    <td>${row.DepartName || ''}</td>
                    <td>${row.UserName || ''}</td>
                    <td>${row.ProblemType || ''}</td>
                    <td>${row.ProcessingType || ''}</td>
                    <td>${row.ProcessingStaff || ''}</td>
                    <td>${toTaiwanDate(row.CompletionDate, "R/MM/dd HH:mm") || ''}</td>
                    <td><button class="btn btn-secondary edit_btn" data-id="${row.RecordId}">編輯</button></td>
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

document.addEventListener('DOMContentLoaded', async function () {

    var searchForm = document.getElementById('searchForm');
    if (searchForm) {

        // 下拉選單初始化
        await initDropdowns();

        searchForm.addEventListener('submit',async function (e) {
            e.preventDefault();

            const formData = new FormData(searchForm);
            if(!validateForm(formData)) return;

            //搜尋並渲染
            await searchAndRender(formData);

            // 渲染表格後，監聽所有編輯按鈕
            document.getElementById('searchResults').addEventListener('click', function (e) {
                if (e.target && e.target.classList.contains('edit_btn')) {
                    const recordId = e.target.getAttribute('data-id');
                    if (recordId) {
                        window.location.href = `/Home/Edit/${recordId}`;
                    }
                }
            });
        });
    }
});