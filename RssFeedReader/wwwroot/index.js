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
                <div class="col-md-6 mb-3 d-flex flex-column">
                    <div class="feed-item card flex-grow-1" data-feed='${JSON.stringify(feed)}'>
                        <div class="card-body">
                            <h4 class="card-title"><a href="${feed.url}" target="_blank">${feed.title}</a></h4>
                            <h7 class="card-subtitle mb-2 text-muted">${new Date(feed.publishDate).toLocaleString()}</h7>
                            <p class="card-text">${feed.feedSource || ''}</p>
                            <p class="card-text">${feed.author || ''}</p>
                            <p class="card-text">${feed.categories && feed.categories !== "null" ? feed.categories : ''}</p>
                        </div>
                    </div>
                </div>
            `;
        });
        $('#feed-container').html(html);

        $('.feed-item').click(function () {
            const feed = $(this).data('feed');
            console.log('Feed:', feed);

            showPopup(feed);
        });
    }

    function showPopup(feed) {
        if (!feed) {
            console.error('Feed is null or undefined:', feed);
            return;
        }

        // NOTE: In come rare sporadic cases, the stringified object contains double quotes.
        // In most cases, it looks like: {id: 2, title : "Title"}, while in problematic cases: {"id": "2", "title": "Title", ...}
        // However, I send the data from the server the same way and even in Postman, it looks the same in case of problematic cases and most cases.

        // Unterminated string caught at line...
        //if (typeof feed === 'string') {
        //    feed = JSON.parse(feed);
        //}

        if (!feed.title) {
            $('#feedModalLabel').text('Invalid title.');
        } else {
            $('#feedModalLabel').text(feed.title);
        }

        let date = ''
        try {
            date = new Date(feed.publishDate).toLocaleString();
        } catch (error) {
            console.error('Error parsing date:', error);
            date = 'Invalid date';
        }

        $('#modal-published').text('Published: ' + date);

        $('#modal-source').text(feed.feedSource);
        $('#modal-author').text(feed.author);
        $('#modal-categories').text(feed.categories || '');

        // NOTE: Some RSS feeds contain HTML, while others contain plain text.
        // Sanitize the content if it contains HTML.
        if (feed.description && containsHtml(feed.description)) {
            const sanitizedContent = DOMPurify.sanitize(feed.description);
            $('#modal-description').html(sanitizedContent);
        } else {
            $('#modal-description').text(feed.description);
        }

        if (feed.content && containsHtml(feed.content)) {
            const sanitizedContent = DOMPurify.sanitize(feed.content);
            $('#modal-content').html(sanitizedContent);
        } else {
            $('#modal-content').text(feed.content);
        }

        $('#modal-article-link').attr('href', feed.url);

        if (feed.imageUrl) {
            $('#modal-image').attr('src', feed.imageUrl).show();
        } else {
            $('#modal-image').hide();
        }

        $('#feedModal').modal('show');
    }

    function updatePagination(totalPages, page) {
        console.log(totalPages)
        let paginationHtml = '';

        if (page > 1) {
            paginationHtml += `<button id="prev-page" class="btn btn-primary" data-page="${page - 1}">Previous</button>`;
        }

        paginationHtml += `<select id="page-select" class="form-control d-inline w-auto mx-2">`;
        for (let i = 1; i <= totalPages; i++) {
            paginationHtml += `<option value="${i}" ${i === page ? 'selected' : ''}>${i}</option>`;
        }
        paginationHtml += `</select>`;

        if (page < totalPages) {
            paginationHtml += `<button id="next-page" class="btn btn-primary" data-page="${page + 1}">Next</button>`;
        }

        $('#pagination-controls').html(paginationHtml);

        $('#pagination-controls').off('click', '#prev-page, #next-page').on('click', '#prev-page, #next-page', function () {
            const newPage = parseInt($(this).data('page'));
            fetchFeeds(newPage, getFilters());
        });

        $('#pagination-controls').off('change', '#page-select').on('change', '#page-select', function () {
            const newPage = parseInt($(this).val());
            fetchFeeds(newPage, getFilters());
        });

        $('#pagination-controls').html(paginationHtml);
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
        console.log('filters:', filters);
        return filters;
    }

    function updateFilter() {
        const filterHtml = `
            <form id="filter-form">
                <div class="form-group">
                    <label for="author">Title</label>
                    <input type="text" class="form-control" id="title" name="title" aria-describedby="title-help">
                </div>
                <div class="form-group">
                    <label for="feedSource">RSS Feed</label>
                    <select class="form-control" id="feedSource" name="feedSource" aria-label="Select RSS feed source">
                        <option value="">Select a feed source</option>
                    </select>
                </div>
                <div class="form-group">
                    <label for="dateFrom">Date from</label>
                    <input type="date" class="form-control" id="dateFrom" name="dateFrom" max="${new Date().toISOString().split('T')[0]}" aria-describedby="dateFrom-help>
                </div>
                <div class="form-group">
                    <label for="dateTo">Date to</label>
                    <input type="date" class="form-control" id="dateTo" name="dateTo" max="${new Date().toISOString().split('T')[0]}" aria-describedby="dateTo-help>
                </div>
                <div class="form-group">
                    <label for="page-size">Page Size</label>
                    <select class="form-control" id="page-size" name="pageSize" aria-label="Select page size">
                        <option value="6">6</option>
                        <option value="10" selected>10</option>
                        <option value="14">14</option>
                    </select>
                </div>
                 <div id="date-error-message" class="text-danger" style="display: none;"></div>
                <button type="submit" class="btn btn-primary">Apply Filters</button>
                <button type="button" id="clear-filters" class="btn btn-secondary">Clear Filters</button>
            </form>
        `;

        $('#filter-controls').html(filterHtml);

        // NOTE: Fetch the feed sources for the dropdown.
        $.ajax({
            url: '/api/sources',
            type: 'GET',
            success: function (data) {
                if (Array.isArray(data)) {
                    const sourceDropdown = $('#feedSource');
                    data.forEach(function (feedSource) {
                        sourceDropdown.append(`<option value="${feedSource}">${feedSource}</option>`);
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
            if (!validateDates(e)) {
                // NOTE: Stop form submission if validation failed.
                return;
            }

            e.preventDefault();
            pageSize = parseInt($('#page-size').val());
            fetchFeeds(currentPage, getFilters());
        });

        $('#dateFrom, #dateTo').on('change', function () {
            validateDates(); // NOTE: Validate on change.
        });

        $('#clear-filters').click(function () {
            pageSize = parseInt($('#page-size').val());
            currentPage = 1;
            $('#filter-form')[0].reset();
            console.log(pageSize)
            fetchFeeds(currentPage, getFilters());
        })
    }

    function validateDates(e) {
        const dateFromValue = $('#dateFrom').val();
        const dateToValue = $('#dateTo').val();
        const errorMessage = $('#date-error-message');

        if (dateFromValue && dateToValue) {
            const dateFrom = new Date(dateFromValue);
            const dateTo = new Date(dateToValue);

            if (dateFrom > dateTo) {
                errorMessage.text('Date from cannot be later than date to!').show();
                if (e) {
                    e.preventDefault();
                }
                return false;
            }
        }
        errorMessage.hide();
        return true;
    }

    function containsHtml(text) {
        return /<(?!\s*\/?\s*(?:area|br|col|embed|hr|img|p|input|link|meta|param)\b)[^>]+>/.test(text);
    }

    // NOTE: Content of the button is 'Filter' by default and 'Collapse' when toggled to avoid confusion.
    $('#filter-button').click(function () {
        $('#filter-controls').toggle();
        if ($('#filter-controls').is(':visible')) {
            $(this).text('Collapse');
        } else {
            $(this).text('Filter');
        }
    });

    // NOTE: This is a workaround to prevent the modal from showing the previous feed content.
    $('#feedModal').on('hidden.bs.modal', function () {
        $('#feedModalLabel').text('');
        $('#modal-published').text('');
        $('#modal-source').text('');
        $('#modal-author').text('');
        $('#modal-categories').text('');
        $('#modal-description').text('');
        $('#modal-content').text('');
        $('#modal-article-link').attr('href', '');
        $('#modal-image').attr('src', '');
    });

    updateFilter();
    fetchFeeds(currentPage);
});