﻿$(document).ready(function () {
    let currentPage = 1;
    let pageSize = 10;

    function loadPage() {
        const token = localStorage.getItem('jwtToken');
        if (token) {
            $('#user-logged-in').show();
            $('#logout-button').show();
            updateFilter();
            fetchFeeds(currentPage);
        } else {
            $('#user-logged-out').show();
            register();
            login();
        }
    }

    function fetchFeeds(page, filters = {}) {
        const filterParams = $.param(filters);
        const token = localStorage.getItem('jwtToken');
        $.ajax({
            url: `/api/news?page=${page}&pageSize=${pageSize}&${filterParams}`,
            headers: {
                'Authorization': `Bearer ${token}`
            },
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
            showPopup(feed);
        });
    }

    function showPopup(feed) {
        $('#feedModalLabel').text(feed.title);
        $('#modal-published').text('Published: ' + new Date(feed.publishDate).toLocaleString());
        $('#modal-source').text(feed.feedSource);
        $('#modal-author').text(feed.author);
        $('#modal-categories').text(feed.categories || '');

        // NOTE: Some feeds contain 'p' tags in the description, remove them.
        let cleanedDescription = removePTags(feed.description);
        $('#modal-description').text(cleanedDescription || '');
        $('#modal-content').text(feed.content || '');
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

        // NOTE: Fetch sources for the dropdown.
        const token = localStorage.getItem('jwtToken');
        $.ajax({
            url: '/api/sources',
            type: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`
            },
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
            validateDates(e); // Call validation function
            if (e.isDefaultPrevented()) {
                return; // Stop form submission if validation failed
            }

            e.preventDefault();
            pageSize = parseInt($('#page-size').val());
            fetchFeeds(currentPage, getFilters());
        });

        $('#dateFrom, #dateTo').on('change', function () {
            validateDates(); // Validate on change
        });

        $('#clear-filters').click(function () {
            pageSize = parseInt($('#page-size').val());
            currentPage = 1;
            $('#filter-form')[0].reset();
            console.log(pageSize)
            fetchFeeds(currentPage, getFilters());
        })
    }

    function register() {
        const registerHtml = `
            <form id="register-form">
                <div class="form-group">
                    <label for="username">User name</label>
                    <input type="text" class="form-control" id="username" name="username" aria-describedby="username-help">
                </div>
                <div class="form-group">
                    <label for="password">Password</label>
                    <input type="password" class="form-control" id="password" name="password" aria-describedby="password-help">
                </div>
               <div class="form-group">
                    <label for="confirmPassword">Confirm password</label>
                    <input type="password" class="form-control" id="confirmPassword" name="confirmPassword" aria-describedby="confirm-password-help">
                </div>
                <div id="pw-error-message" class="text-danger" style="display: none;"></div>
                <button type="submit" class="btn btn-primary">Register</button>
                <button type="button" id="clear-register-form" class="btn btn-secondary">Clear</button>
            </form>
        `;

        $('#register-controls').html(registerHtml);

        $('#register-form').on('submit', function (e) {
            e.preventDefault();
            registerUser();
        });

        $('#clear-register-form').click(function () {
            $('#filter-form')[0].reset();
        })
    }

    function login() {
        const loginHtml = `
            <form id="login-form">
                <div class="form-group">
                    <label for="username">User name</label>
                    <input type="text" class="form-control" id="usernamel" name="username" aria-describedby="username-help">
                </div>
                <div class="form-group">
                    <label for="password">Password</label>
                    <input type="password" class="form-control" id="passwordl" name="password" aria-describedby="password-help">
                </div>
                <button type="submit" class="btn btn-primary">Log in</button>
            </form>
        `;

        $('#login-controls').html(loginHtml);

        $('#login-form').on('submit', function (e) {
            e.preventDefault();
            loginUser();
        });
    }

    function registerUser() {
        const username = $('#username').val();
        const password = $('#password').val();
        const confirmPassword = $('#confirmPassword').val();
        if (password !== confirmPassword) {
            alert('Passwords do not match!');
            return;
        }
        $.ajax({
            url: '/api/account/register',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                username: username,
                password: password
            }),
            success: function (data) {
                console.log('User registered:', data);
                alert('User registered successfully!');
                localStorage.setItem('jwtToken', data.token);
                location.reload();
                $('#logout-button').show();
            },
            error: function (error) {
                console.error('Error registering user:', error);
                alert('Error registering user!');
            }
        });
    }

    function loginUser() {
        const username = $('#usernamel').val();
        const password = $('#passwordl').val();
        $.ajax({
            url: '/api/account/login',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                username: username,
                password: password
            }),
            success: function (data) {
                console.log('User logged in:', data);
                localStorage.setItem('jwtToken', data.token);
                location.reload();
                $('#logout-button').show();
            },
            error: function (error) {
                console.error('Error logging in user:', error);
                alert('Error logging in user!');
            }
        });
    }

    function removePTags(description) {
        if (!description) {
            return ""; // Handle null or undefined descriptions
        }
        return description.replace(/<p>|<\/p>/g, '');
    }

    function validateDates(e) {
        const dateFromValue = $('#dateFrom').val();
        const dateToValue = $('#dateTo').val();
        const errorMessage = $('#date-error-message');

        if (dateFromValue && dateToValue) {
            const dateFrom = new Date(dateFromValue);
            const dateTo = new Date(dateToValue);

            if (dateFrom > dateTo) {
                errorMessage.text('"Date from" cannot be later than "date to"!').show();
                alert('"Date from" cannot be later than "date to"!');
                if (e) {
                    e.preventDefault();
                }
            }
        }
        errorMessage.hide();
    }

    // NOTE: Event listeners to hide or show the base elements of the page.

    $('#filter-button').click(function () {
        $('#filter-controls').toggle();
    });

    $('#register-button').click(function () {
        $('#login-button').toggle();
        $('#register-controls').toggle();
    });

    $('#login-button').click(function () {
        $('#register-button').toggle();
        $('#login-controls').toggle();
    });

    $('#logout-button').click(function () {
        localStorage.removeItem('jwtToken');
        location.reload();
    });

    loadPage();
});