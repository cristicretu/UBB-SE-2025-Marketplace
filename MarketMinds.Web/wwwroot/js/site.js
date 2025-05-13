// This file contains JavaScript functions for the ASP.NET Core MVC part
// The React app has its own JavaScript in the ClientApp directory

// Wait for the document to be fully loaded
document.addEventListener("DOMContentLoaded", function () {
  console.log("MVC site.js loaded");

  // Add any global event listeners or functionality for the MVC parts here
  const navLinks = document.querySelectorAll("nav a");

  navLinks.forEach((link) => {
    link.addEventListener("mouseover", function () {
      this.style.transition = "color 0.3s";
    });
  });
});

// Example function for use in MVC views
function toggleVisibility(elementId) {
  const element = document.getElementById(elementId);
  if (element) {
    element.style.display = element.style.display === "none" ? "block" : "none";
  }
}

// Site-wide JavaScript functionality

// DOM Ready
document.addEventListener("DOMContentLoaded", function () {
  console.log("MarketMinds application initialized");

  // Initialize countdown timers if they exist
  initAuctionCountdowns();
});

// Function to initialize auction countdowns
function initAuctionCountdowns() {
  const countdownElements = document.querySelectorAll("[data-countdown]");

  if (countdownElements.length === 0) return;

  countdownElements.forEach((element) => {
    const endTimeStr = element.getAttribute("data-countdown");
    if (!endTimeStr) return;

    const endTime = new Date(endTimeStr).getTime();

    // Update immediately and then every second
    updateCountdown(element, endTime);
    setInterval(() => updateCountdown(element, endTime), 1000);
  });
}

// Update a single countdown element
function updateCountdown(element, endTime) {
  const now = new Date().getTime();
  const timeLeft = endTime - now;

  if (timeLeft <= 0) {
    element.textContent = "Auction Ended";
    element.classList.add("text-red-600");
    return;
  }

  // Calculate time components
  const days = Math.floor(timeLeft / (1000 * 60 * 60 * 24));
  const hours = Math.floor(
    (timeLeft % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60)
  );
  const minutes = Math.floor((timeLeft % (1000 * 60 * 60)) / (1000 * 60));
  const seconds = Math.floor((timeLeft % (1000 * 60)) / 1000);

  // Display the countdown
  element.textContent = `${days}d ${hours}h ${minutes}m ${seconds}s`;
}
