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
*   **Document Routing:** A new section for managing the document routing sequence based on document type, department, section, and plant.

## Implemented Features

*   **Replaced ASP.NET Core Identity with a custom cookie-based authentication system.**
*   **Switched to Username-based Authentication.**
*   **Improved User Deletion.**
*   **Added Auditing to User Management.**
*   **Enhanced Master Data Management.**
*   **Added Document Routing Feature.**
    *   Created the `DocumentType` and `DocumentRouting` models and updated the database.
    *   Developed the `DocumentRoutingController` with `Create`, `Edit`, and `Delete` actions.
    *   Created the `Index`, `Edit`, and `Delete` views for document routing management.
    *   Added a navigation link to the "Document Routing" page.
    *   Refactored the `DocumentRoutingViewModel` and `Create` action to prevent null reference exceptions.
    *   Updated the `DocumentRoutingController` to use the `RequestType` enum for the "Document Type" dropdown, ensuring consistency with the `Create Request` page.
    *   Resolved a validation issue on the Document Routing creation form by initializing list properties in the `DocumentRoutingViewModel`.
    *   Refactored the `Create` action in the `DocumentRoutingController` to use a specific `CreateDocumentRoutingViewModel` for form binding, resolving validation errors.

## Current Plan

(No active plan)
