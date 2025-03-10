$(document).ready(function () {
    let currentPage = 1;
    const pageSize = 10;

    function fetchFeeds(page) {
        $.ajax({
            url: `/api/news/filter?page=${page}&pageSize=${pageSize}`,
            type: 'GET',
            success: function (data) {
                displayFeeds(data.items);
                updatePagination(data.totalPages, page);
            },
            error: function (error) {
                console.error('Error fetching feeds:', error);
                $('#feed-container').html('<p>Error loading feeds.</p>');
            }
        });
    }

    function displayFeeds(feeds) {
        let html = '';
        feeds.forEach(function (feed) {
            html += `
                <div class="feed-item">
                    <h3><a href="${feed.url}" target="_blank">${feed.title}</a></h3>
                    ${new Date(feed.publishDate).toLocaleString()}
                    ${feed.author}<br>
                    ${feed.description}
                </div>
            `;
        });
        $('#feed-container').html(html);
    }

    function updatePagination(totalPages, currentPage) {
        let paginationHtml = '';

        if (currentPage > 1) {
            paginationHtml += `<button id="prev-page" class="btn btn-primary">Previous</button>`;
        }

        if (currentPage < totalPages) {
            paginationHtml += `<button id="next-page" class="btn btn-primary">Next</button>`;
        }

        $('#pagination-controls').html(paginationHtml);

        $('#prev-page').click(function () {
            fetchFeeds(currentPage - 1);
        });

        $('#next-page').click(function () {
            fetchFeeds(currentPage + 1);
        });
    }

    fetchFeeds(currentPage);
});