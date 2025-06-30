document.addEventListener('DOMContentLoaded', function() {
    var searchForm = document.getElementById('searchForm');
    if (searchForm) {
        searchForm.addEventListener('submit', function(e) {
            e.preventDefault();

            const formData = new FormData(searchForm);

            fetch('/Home/Search', {
                method: 'POST',
                body: formData
            })
            .then(res => res.json())
            .then(data => {
                let html = '';
                if (data.length === 0) {
                    html = '<div>查無資料</div>';
                } else {
                    html = '<table class="table table-bordered"><thead><tr>' +
                        '<th>申報日期</th><th>完成日期</th><th>使用單位</th><th>問題類別</th><th>處理人員</th><th>處理類別</th>' +
                        '</tr></thead><tbody>';
                    data.forEach(row => {
                        html += `<tr>
                            <td>${row.reportDate || ''}</td>
                            <td>${row.finishDate || ''}</td>
                            <td>${row.department || ''}</td>
                            <td>${row.issueType || ''}</td>
                            <td>${row.handler || ''}</td>
                            <td>${row.handleType || ''}</td>
                        </tr>`;
                    });
                    html += '</tbody></table>';
                }
                document.getElementById('searchResults').innerHTML = html;
            })
            .catch(err => {
                document.getElementById('searchResults').innerHTML = '<div class="text-danger">查詢失敗：' + err + '</div>';
            });
        });
    }
});