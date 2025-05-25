-- Create Bot User for ChatBot System
-- This script creates a system user with ID 0 for the chatbot functionality
-- Run this script once after setting up the database

-- First, enable IDENTITY_INSERT to allow inserting a specific ID
SET IDENTITY_INSERT Users ON;

-- Insert the bot user with ID 0
INSERT INTO Users (id, username, email, phoneNumber, passwordHash, userType, balance, failedLogIns, bannedUntil, isBanned)
VALUES (0, 'ChatBot Assistant', 'chatbot@marketminds.com', '', '', 0, 0.0, 0, NULL, 0);

-- Disable IDENTITY_INSERT
SET IDENTITY_INSERT Users OFF;

-- Verify the bot user was created
SELECT * FROM Users WHERE id = 0;

PRINT 'Bot user with ID 0 has been created successfully!'; 