$(document).ready(function () {
    let currentPage = 1;
    const pageSize = 10;

    function fetchFeeds(page) {
        $.ajax({
            url: `/api/news?page=${page}&pageSize=${pageSize}`,
            type: 'GET',
            success: function (data) {
                console.log(data.items);
                console.log(data.totalPages);
                if (data && Array.isArray(data.items)) {
                    displayFeeds(data.items);
                    updatePagination(data.totalPages, page);
                } else {
                    console.error('API response does not contain valid items:', data);
                    $('#feed-container').html('<p class="alert alert-danger">Error: No items found.</p>');
                }
            },
            error: function (error) {
                console.error('Error fetching feeds:', error);
                $('#feed-container').html('<p class="alert alert-danger">Error loading feeds.</p>');
            }
        });
    }

    function displayFeeds(feeds) {
        let html = '';
        feeds.forEach(function (feed) {
            html += `
                <div class="feed-item card mb-3">
                    <div class="card-body">
                        <h3 class="card-title"><a href="${feed.url}" target="_blank">${feed.title}</a></h3>
                        <h6 class="card-subtitle mb-2 text-muted">${new Date(feed.publishDate).toLocaleString()}</h6>
                        <p class="card-text">${feed.author}</p>
                        <p class="card-text">${feed.description}</p>
                    </div>
                </div>
            `;
        });
        $('#feed-container').html(html);
    }

    function updatePagination(totalPages, page) {
        console.log(totalPages)
        let paginationHtml = '';

        if (page > 1) {
            paginationHtml += `<button id="prev-page" class="btn btn-primary" data-page="${page - 1}">Previous</button>`;
        }

        if (page < totalPages) {
            paginationHtml += `<button id="next-page" class="btn btn-primary" data-page="${page + 1}">Next</button>`;
        }

        $('#pagination-controls').html(paginationHtml);

        // Event delegation
        $('#pagination-controls').off('click', '#prev-page, #next-page').on('click', '#prev-page, #next-page', function () {
            const newPage = parseInt($(this).data('page'));
            fetchFeeds(newPage);
        });
    }

    fetchFeeds(currentPage);
});