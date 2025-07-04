//西元年轉換民國年
function toTaiwanDate(isoDateStr, format = "R/MM/dd HH:mm") {
    if (!isoDateStr) return "";
    const dtmDate = new Date(isoDateStr);
    if (isNaN(dtmDate)) return isoDateStr;
    const intYear = dtmDate.getFullYear() - 1911;
    const intMonth = (dtmDate.getMonth() + 1).toString().padStart(2, "0");
    const strDay = dtmDate.getDate().toString().padStart(2, "0");
    const strHour = dtmDate.getHours().toString().padStart(2, "0");
    const strMin = dtmDate.getMinutes().toString().padStart(2, "0");
    return format
        .replace("R", intYear)
        .replace("MM", intMonth)
        .replace("dd", strDay)
        .replace("HH", strHour)
        .replace("mm", strMin);
}

//民國年轉換西元年
function rocToAD(strROC) {
    // 1130701 → 2024-07-01
    if (!strROC) return "";
    const m = strROC.match(/^(\d{3})(\d{2})(\d{2})$/);
    if (!m) return "";
    const intYear = parseInt(m[1], 10) + 1911;
    const intMonth = m[2];
    const strDay = m[3];
    return `${intYear}-${intMonth}-${strDay}`;
}

function mergeDateAndTime(strDate, strTime) {

    if (strDate && strTime) {
        // 轉西元年月日
        const adDate = rocToAD(strDate);

        const strHour = strTime.substring(0, 2);
        const strMinute = strTime.substring(2, 4);

        const dateTimeStr = `${adDate}T${strHour}:${strMinute}`; // ISO格式
        return dateTimeStr;
    }

    return null;
}

function showErrors(objMessage) {
    if (!objMessage) {
        alert('發生未知錯誤');
        return;
    }
    if (Array.isArray(objMessage)) {
        alert(objMessage.join('\n'));
    } else if (typeof objMessage === 'object') {
        // 嘗試抓取常見錯誤屬性
        if (objMessage.errors && Array.isArray(objMessage.errors)) {
            alert(objMessage.errors.join('\n'));
        } else if (objMessage.objMessage) {
            alert(objMessage.objMessage);
        } else {
            alert(JSON.stringify(objMessage));
        }
    } else {
        alert(objMessage);
    }
}


//依據選取的單位列出人員
async function getStaffsByDepartment(strDepartCode, strStaffSelectId) {

    const staffSelect = document.getElementById(strStaffSelectId);
    staffSelect.innerHTML = '<option value="">載入中...</option>';

    try {
        const res = await fetch(`/Home/dropdown/staffs?depart_code=${encodeURIComponent(strDepartCode)}`);
        const data = await res.json();
        staffSelect.innerHTML = '<option value="">請選擇</option>';
        data.forEach(item => {
            staffSelect.innerHTML += `<option value="${item.Value}">${item.Text}</option>`;
        });
    } catch (err) {
        staffSelect.innerHTML = '<option value="">載入失敗</option>';
    }

}

function isValidRocDate(rocStr) {
    if (!/^[0-9]{7}$/.test(rocStr)) return false;
    const y = parseInt(rocStr.substring(0,3), 10) + 1911;
    const m = parseInt(rocStr.substring(3,5), 10);
    const d = parseInt(rocStr.substring(5,7), 10);
    if (m < 1 || m > 12 || d < 1 || d > 31) return false;
    // 檢查是否為有效日期
    const date = new Date(y, m - 1, d);
    return date.getFullYear() === y && date.getMonth() === m - 1 && date.getDate() === d;
}