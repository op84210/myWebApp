
document.addEventListener('DOMContentLoaded', async function () {

    var maintainForm = document.getElementById('maintainForm');
    if (maintainForm) {

        var mode = maintainForm.getAttribute('data-mode');
        maintainForm.addEventListener('submit',async function (e) {
            e.preventDefault();

            const formData = new FormData(maintainForm);
            if(!validateForm(formData)) return;

            console.log(Object.fromEntries(formData.entries()));
            
            if(mode === 'Edit'){
                await edit(formData);
            }else{
                await create(formData);
            }
            

        });
    }
    
});

function validateForm(formData) {

    // 取得民國年欄位
    var apply_date_roc = formData.get('apply_date_roc');
    var completion_date_roc = formData.get('completion_date_roc');

    // 驗證格式
    const dateFields = [
        { id: 'apply_date_roc', value: apply_date_roc },
        { id: 'completion_date_roc', value: completion_date_roc }
    ];

    for (const field of dateFields) {
        if (!field.value || !/^\d{7}$/.test(field.value)) {
            alert('請輸入正確的民國年格式（如1130701）');
            document.getElementById(field.id).focus();
            return false;
        }
    }

    // 取得時間欄位
    var apply_time = formData.get('apply_time');
    var completion_time = formData.get('completion_time');
        
    const timeFields = [
        { id: 'apply_time', value: apply_time, name: '申報時間' },
        { id: 'completion_time', value: completion_time, name: '完成時間' }
    ];

    for (const field of timeFields) {
        if (field.value) {
            if (!/^\d{4}$/.test(field.value)) {
                alert(field.name + '請輸入4碼數字（如0930）');
                document.getElementById(field.id).focus();
                return false;
            }
            const hour = parseInt(field.value.substring(0, 2), 10);
            const minute = parseInt(field.value.substring(2, 4), 10);
            if (hour < 0 || hour > 23 || minute < 0 || minute > 59) {
                alert(field.name + '請輸入正確的24小時制時間（如0930、2359）');
                document.getElementById(field.id).focus();
                return false;
            }
        }
    }

    // 轉換成西元年並存到FormData
    var apply_date= mergeDateAndTime(apply_date_roc,apply_time);
    formData.set('apply_date', apply_date);

    var completion_date= mergeDateAndTime(completion_date_roc,completion_time);
    formData.set('completion_date', completion_date);

    return true;
}

function mergeDateAndTime(dateStr, timeStr){

    if (dateStr && timeStr) {
        // 轉西元年月日
        const adDate = rocToAD(dateStr);
        
        // 轉成 yyyy-MM-dd HH:mm 格式
        const hour = timeStr.substring(0, 2);
        const minute = timeStr.substring(2, 4);

        const dateTimeStr = `${adDate}T${hour}:${minute}`; // ISO格式
        return dateTimeStr;
    }

    return null;
}

//儲存
async function edit(formData) {
    try {
        const res = await fetch('/Home/Edit', {
            method: 'POST',
            body: formData
        });
        if (!res.ok) throw new Error('儲存失敗');
        const data = await res.json();
        console.log(data);

    } catch (err) {

    }
}

async function create(formData) {
    try {
        const res = await fetch('/Home/Create', {
            method: 'POST',
            body: formData
        });
        if (!res.ok) throw new Error('儲存失敗');
        const data = await res.json();
        console.log(data);

    } catch (err) {

    }
}