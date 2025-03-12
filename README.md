# RSS Feed Reader

## How to Use It:

- Add a system environment variable named `TOKEN_KEY` with a length of at least 64 characters.
- Add a connection string named `DefaultConnection` to the `appsettings.Development.json` file.
- After completing these steps, run the `Update-Database` command.
- The data from the four RSS feeds will be populated upon the first application start.

## Note:

This product is still in the development phase.
Before deploying it to production, several security considerations are necessary:

- Deploy the database to a shared server or to Azure Cloud.
- Store secrets securely, for example, in the cloud.
- Address any potential SSL/certificate issues.

## Technology Stack:

- .NET 8
- HTML, jQuery
- Bootstrap
- Entity Framework Core
- MS SQL

## Branches

- **`master`:**
    - Contains the implementation of the required task: gathering articles from four RSS feed sources, saving them to the database, fetching them, and displaying them to users.
- **`feature`:**
    - Contains the implementation of the optional task: user registration/login functionality and statistics gathering.

## Detailed Documentation:

- Users can register, or sign in if they already have an account.
- Currently, only the username and password are required.
- The authentication token expires after two days.
- For signed-in users, the RSS feed is displayed.
- Initially, articles from the last seven days are loaded, assuming the RSS servers are functioning correctly.
- The feed can be filtered by title, feed source (from the default four), and date range (from and to).
- To minimize server interactions, the feed is only updated when the "Apply Filters" button is clicked.
- Filters can be reset using the "Reset Filters" button.
- Pagination is available with "Next" and "Previous" buttons. Users can also select a specific page from a dropdown menu, which defaults to the current page.
- Clicking on any feed card displays a pop-up with the article details. The "Read Original Article" button opens the original article in a new tab.
- The total number of views is tracked, but not individually per user.

## Note for Developers:

- The code may contain suboptimal practices and may not be perfectly organized in all areas.
