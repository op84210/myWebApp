document.addEventListener('DOMContentLoaded', async function () {
    initFormSubmit();
    initDeleteButton();
    initDepartmentDropdown();
});

function initFormSubmit() {
    const maintainForm = document.getElementById('maintainForm');
    if (maintainForm) {
        const strMode = maintainForm.getAttribute('data-mode');
        maintainForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            const formData = new FormData(maintainForm);
            if (!validateForm(formData)) return;
            if (strMode === 'Edit') {
                await edit(formData);
            } else {
                await create(formData);
            }
        });
    }
}

function initDeleteButton() {
    const delBtn = document.getElementById('deleteBtn');
    if (delBtn) {
        delBtn.addEventListener('click', async function () {
            if (confirm('確定要刪除這筆資料嗎？')) {
                const strRecordId = document.getElementById('record_id').value;
                await deleteData(strRecordId);
            }
        });
    }
}

function initDepartmentDropdown() {
    const depart_code = document.getElementById('depart_code');
    if (depart_code) {
        depart_code.addEventListener('change', async function () {
            await getStaffsByDepartment(this.value, 'staff_id');
        });
    }
}

function validateForm(formData) {

    if (!validateRocDates(formData)) return false;
    if (!validateTimes(formData)) return false;
    if (!validateProcessingTime(formData)) return false;

    // 轉換成西元年並存到FormData
    const strApplyDate = mergeDateAndTime(formData.get('apply_date_roc'), formData.get('apply_time'));
    formData.set('apply_date', strApplyDate);
    const strCompletionDate = mergeDateAndTime(formData.get('completion_date_roc'), formData.get('completion_time'));
    formData.set('completion_date', strCompletionDate);

    //計算處理時間
    const intHours = parseInt(formData.get('hours'), 10) || 0;
    const intMinutes = parseInt(formData.get('minutes'), 10) || 0;
    const intProcessingMinutes = intHours * 60 + intMinutes;
    formData.set('processing_minutes', intProcessingMinutes);

    return true;
}

function validateRocDates(formData) {

    const strApplyDateROC = formData.get('apply_date_roc');
    const strCompletionDateROC = formData.get('completion_date_roc');

    const aryDateFields = [
        { id: 'apply_date_roc', value: strApplyDateROC },
        { id: 'completion_date_roc', value: strCompletionDateROC }
    ];

    for (const field of aryDateFields) {
        if (field.value && !isValidRocDate(field.value)) {
            alert('請輸入有效的日期（如1130701）');
            document.getElementById(field.id).focus();
            return false;
        }
    }

    return true;
}

function validateTimes(formData) {

    const strApplyTime = formData.get('apply_time');
    const strCompletionTime = formData.get('completion_time');

    const aryTimeFields = [
        { id: 'apply_time', value: strApplyTime, name: '申報時間' },
        { id: 'completion_time', value: strCompletionTime, name: '完成時間' }
    ];

    for (const field of aryTimeFields) {
        if (field.value) {
            if (!isValidTime(field.value)) {
                alert(field.name + '請輸入正確的24小時制時間（如0930、2359）');
                document.getElementById(field.id).focus();
                return false;
            }
        }
    }

    return true;
}

function isValidTime(str) {
    return /^\d{4}$/.test(str) &&
        parseInt(str.substring(0, 2), 10) >= 0 &&
        parseInt(str.substring(0, 2), 10) <= 23 &&
        parseInt(str.substring(2, 4), 10) >= 0 &&
        parseInt(str.substring(2, 4), 10) <= 59;
}

function validateProcessingTime(formData) {

    const intHours = parseInt(formData.get('hours'), 10) || 0;
    const intMinutes = parseInt(formData.get('minutes'), 10) || 0;

    if (intHours < 0 || intMinutes < 0 || intMinutes > 59) {
        alert('請輸入正確的處理時間（小時需>=0，分鐘0~59）');
        return false;
    }

    return true;
}

//編輯
async function edit(formData) {
    try {
        const res = await fetch('/Home/Edit', {
            method: 'POST',
            body: formData
        });

        const data = await res.json();
        if (!data.success) {
            showErrors(data.errors);
            return;
        }

        alert('編輯成功');
        window.location.href = '/Home/Index'; // 自動導回主頁

    } catch (err) {
        alert('編輯失敗');
    }
}

//新增
async function create(formData) {
    try {
        const res = await fetch('/Home/Create', {
            method: 'POST',
            body: formData
        });

        const data = await res.json();
        if (!data.success) {
            showErrors(data.errors);
            return;
        }

        alert('儲存成功');
        window.location.href = '/Home/Index'; // 自動導回主頁

    } catch (err) {
        alert('儲存失敗');
    }
}

//刪除
async function deleteData(strRecordId) {
    try {
        const res = await fetch('/Home/Delete', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ "record_id": strRecordId })
        });

        const data = await res.json();
        if (!data.success) {
            showErrors(data.message);
            return;
        }

        alert('刪除成功');
        window.location.href = '/Home/Index'; // 自動導回主頁

    } catch (err) {
        alert('刪除失敗');
    }
}

