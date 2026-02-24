# Blueprint: SAP Service Web App

## Overview

This document outlines the blueprint for a .NET-based web application designed to streamline SAP service requests. The application will provide a user-friendly interface for creating, tracking, and managing service requests, with a focus on clear separation of concerns and a robust, scalable architecture.

## Project Outline

### Style and Design

*   **Framework:** Bootstrap 5 for a responsive, mobile-first layout.
*   **Font:** "Poppins" from Google Fonts for a clean, modern aesthetic.
*   **Icons:** Font Awesome for intuitive user interface elements.
*   **Color Palette:** A professional and visually balanced color scheme will be implemented.

### Features

*   **User Authentication:** Secure user login and registration based on the `Users` table.
*   **Request Management:** Create, view, edit, and delete service requests.
*   **Master Data Management:** A dedicated section for managing master data related to requests.
*   **User Management:** An administrative interface for managing user accounts.

## Implemented Features

*   **Replaced ASP.NET Core Identity with a custom cookie-based authentication system.**
    *   Created a `Login` view and `AccountController` to handle user login.
    *   Configured cookie authentication in `Program.cs`.
    *   Updated the layout to dynamically show "Login" and "Logout" links.
    *   Secured all controllers using the `[Authorize]` attribute.
    *   Refactored the `UsersController` to remove dependencies on the old Identity system.
    *   Removed unused files and dependencies from the old Identity system.
*   **Switched to Username-based Authentication.**
    *   Added `UserName` to the `ApplicationUser` model.
    *   Updated the `CreateUserViewModel` and `EditUserViewModel` to include `UserName`.
    *   Updated the `Create.cshtml` and `Edit.cshtml` views to include a `UserName` field.
    *   Modified the `UsersController` to handle the `UserName` when creating and editing users.
    *   Updated the `LoginViewModel` to use `UserName` instead of `Email`.
    *   Updated the `Login.cshtml` view to ask for a `UserName`.
    *   Modified the `AccountController` to use `UserName` for authentication.
*   **Improved User Deletion.**
    *   Updated the `DeleteConfirmed` action in the `UsersController` to properly handle deletion errors and provide feedback to the user.
*   **Added Auditing to User Management.**
    *   The `UsersController` now automatically records the creator and updater of a user, along with timestamps for creation and updates.
*   **Enhanced Master Data Management.**
    *   Added `Edit` and `Delete` functionality to the `MasterDataController`.
    *   Created `Edit` and `Delete` views for master data combinations.
    *   Added "Edit" and "Delete" buttons to the master data management page, allowing for direct management of combinations.

## Current Plan

(No active plan)
