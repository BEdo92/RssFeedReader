$(document).ready(function () {
    let currentPage = 1;
    let pageSize = 10;

    function fetchFeeds(page, filters = {}) {
        const filterParams = $.param(filters);
        $.ajax({
            url: `/api/news?page=${page}&pageSize=${pageSize}&${filterParams}`,
            type: 'GET',
            success: function (data) {
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
                        <h4 class="card-title"><a href="${feed.url}" target="_blank">${feed.title}</a></h4>
                        <h7 class="card-subtitle mb-2 text-muted">${new Date(feed.publishDate).toLocaleString()}</h7>
                        <p class="card-text">${feed.feedSource}</p>
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
            fetchFeeds(newPage, getFilters());
        });
    }

    function getFilters() {
        const filters = {};
        $('#filter-controls input, #filter-controls select').each(function () {
            const name = $(this).attr('name');
            const value = $(this).val();
            if (value) {
                filters[name] = value;
            }
        });
        return filters;
    }

    function updateFilter() {
        const filterHtml = `
            <form id="filter-form">
                <div class="form-group">
                    <label for="author">Author</label>
                    <input type="text" class="form-control" id="author" name="author">
                </div>
                <div class="form-group">
                    <label for="feedSource">Source</label>
                    <select class="form-control" id="feedSource" name="feedSource">
                        <option value="">Select a feed source</option>
                    </select>
                </div>
                <div class="form-group">
                    <label for="page-size">Page Size</label>
                    <select class="form-control" id="page-size" name="pageSize">
                        <option value="5">5</option>
                        <option value="10" selected>10</option>
                        <option value="15">15</option>
                    </select>
                </div>
                <button type="submit" class="btn btn-primary">Apply Filters</button>
            </form>
        `;

        $('#filter-controls').html(filterHtml);

        // Fetch sources for the dropdown
        $.ajax({
            url: '/api/sources',
            type: 'GET',
            success: function (data) {
                if (Array.isArray(data)) {
                    const sourceDropdown = $('#source');
                    data.forEach(function (source) {
                        sourceDropdown.append(`<option value="${source}">${source}</option>`);
                    });
                } else {
                    console.error('API response does not contain valid sources:', data);
                }
            },
            error: function (error) {
                console.error('Error fetching sources:', error);
            }
        });

        $('#filter-form').on('submit', function (e) {
            e.preventDefault();
            pageSize = parseInt($('#page-size').val());
            fetchFeeds(currentPage, getFilters());
        });
    }

    $('#filter-button').click(function () {
        $('#filter-controls').toggle();
    });

    $('#clear-filters').click(function () {
        $('#filter-form')[0].reset();
        fetchFeeds(currentPage);
    })

    updateFilter();
    fetchFeeds(currentPage);
});