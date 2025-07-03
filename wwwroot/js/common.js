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

function mergeDateAndTime(dateStr, timeStr) {

    if (dateStr && timeStr) {
        // 轉西元年月日
        const adDate = rocToAD(dateStr);

        const hour = timeStr.substring(0, 2);
        const minute = timeStr.substring(2, 4);

        const dateTimeStr = `${adDate}T${hour}:${minute}`; // ISO格式
        return dateTimeStr;
    }

    return null;
}

function showErrors(message) {
    if (!message) {
        alert('發生未知錯誤');
        return;
    }
    if (Array.isArray(message)) {
        alert(message.join('\n'));
    } else if (typeof message === 'object') {
        // 嘗試抓取常見錯誤屬性
        if (message.errors && Array.isArray(message.errors)) {
            alert(message.errors.join('\n'));
        } else if (message.message) {
            alert(message.message);
        } else {
            alert(JSON.stringify(message));
        }
    } else {
        alert(message);
    }
}