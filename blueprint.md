
# Project Blueprint

## 1. Overview

This project is a full-stack web application built with .NET for managing internal company requests (e.g., new materials, BOM changes). It features user authentication, role-based permissions, and a request-approval workflow.

## 2. Core Features & Design (As of Initial Implementation)

*   **Framework:** ASP.NET Core MVC with Entity Framework Core.
*   **Database:** SQLite for local development.
*   **Authentication:** ASP.NET Core Identity for user login and registration.
*   **Functionality:**
    *   Users can create, view, edit, and delete material requests.
    *   Requests have different types (FG, SM, RM, etc.).
    *   Dynamic forms that adapt to the selected request type.
    *   Excel data import for creating requests in bulk.
    *   A document routing system for multi-step approvals based on department and section.
    *   Role-based access control:
        *   Standard users can create requests with a "Pending" status.
        *   "IT" role users can set the status of a request upon creation and edit the status of existing requests.
*   **Initial Design:**
    *   Default Bootstrap 5 styling.
    *   Standard table-based layout for lists.
    *   Basic form layouts.
    *   Default color scheme.

---

## 3. Current Task: Orange/White Modern UI Redesign

### Plan Overview

The goal is to transform the application's visual identity into a modern, smooth, and aesthetically pleasing interface with a vibrant orange and clean white color scheme. This involves a full overhaul of the layout, typography, and component styling.

### Actionable Steps

1.  **Establish Color Palette & Fonts:**
    *   **Primary Color:** Vibrant Orange (`#F97316`) for interactive elements.
    *   **Background Color:** Light Gray (`#F9FAFB`) for the main body and White (`#FFFFFF`) for content cards/containers.
    *   **Text Color:** Dark Gray (`#1F2937`) for high readability.
    *   **Font:** Implement 'Poppins' from Google Fonts for a clean, modern feel.

2.  **Create a New Stylesheet:**
    *   Create `wwwroot/css/orange-theme.css` to house all the new design rules, keeping them separate and manageable.

3.  **Update Master Layout (`_Layout.cshtml`):**
    *   Link the new Google Font and the `orange-theme.css` file.
    *   Restructure the `<nav>` bar for a cleaner look.
    *   Add a simple, clean `<footer>`.
    *   Apply the new background color to the `<body>`.

4.  **Redesign Core Pages:**
    *   **Index Page (`Index.cshtml`):**
        *   Transform the request list from a standard table into a grid of "cards".
        *   Each card will represent a request, displaying key information and action buttons (`Edit`, `Details`, `Delete`).
        *   Style action buttons with the new orange theme.
    *   **Create/Edit Pages (`Create.cshtml`, `Edit.cshtml`):**
        *   Restyle all form elements (`<input>`, `<select>`, `<textarea>`, `<button>`) for a consistent and modern look.
        *   Organize the form layout into a clean, centered card.
        *   Ensure the dynamic form sections are styled correctly.
    *   **Details Page (`Details.cshtml`):**
        *   Present the request details in a well-structured and styled card format for easy reading.

5.  **Push to GitHub:**
    *   Once the redesign is complete and verified, commit and push all changes to the remote repository.
