# Angular Weather Application

A responsive weather forecasting dashboard built with Angular v22, displaying a 5-day weather forecast consumed from a public REST API. This application includes modern styling (glassmorphism), loading/error handling, and is designed to deploy automatically to GitHub Pages using GitHub Actions.

## Angular Version
- **Angular CLI & Core**: v22.0.5
- **Unit Testing**: Vitest

## Features
- **Live API Integration**: Retrieves weather forecasts from a remote REST API on load.
- **Responsive Design**: Mobile-friendly, adaptive table and layout structure.
- **Loading & Error Handling**: Displays a clear loading spinner and provides a retry mechanism on failure.
- **Bonus Feature 1**: Weather forecast record counter (displays the total count of forecasts loaded).
- **Bonus Feature 2**: High-temperature highlight (any row with temperature > 30&deg;C is styled with a distinct warning background color).
- **Bonus Feature 3**: Refresh button (allows the user to manually trigger a data reload at any time).

## Installation & Setup

1. **Clone the repository**:
   ```bash
   git clone <your-repository-url>
   cd <repository-directory>
   ```

2. **Install dependencies**:
   ```bash
   npm install
   ```

3. **Run locally**:
   ```bash
   npm start
   ```
   Open your browser to `http://localhost:4200` to view the running application.

4. **Run Unit Tests**:
   ```bash
   npm test
   ```

5. **Build the application**:
   ```bash
   npm run build
   ```

## Deployment URL
The application is automatically built and deployed via GitHub Actions to GitHub Pages.
- **Public URL**: `https://<github-username>.github.io/weather-app/`
