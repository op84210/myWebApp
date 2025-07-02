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

