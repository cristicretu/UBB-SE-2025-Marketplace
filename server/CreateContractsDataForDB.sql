GO

SELECT * FROM Buyers
SELECT * FROM Sellers

INSERT INTO Sellers
VALUES (1, 'mike2', 10, 'Razvan for sale', 'Premium quality razvans', 'everywhere', 10)

-- set to your seller id
DECLARE @sellerID int;
SET @sellerID = 3

SELECT * FROM PredefinedContracts

INSERT INTO PredefinedContracts
VALUES
    ('This is a standard artwork purchase contract that outlines the terms and conditions...'),
    ('This is a premium artwork purchase contract with extended warranty...'),
    ('This is a custom artwork commission contract...');

UPDATE PredefinedContracts
SET ContractContent='PURCHASE AGREEMENT
Contract ID: {ContractID}
Order Reference: {OrderID}

THIS PURCHASE AGREEMENT (the "Agreement") is made and entered into on {AgreementDate} (the "Effective Date"),

BETWEEN:
{SellerName} ("Seller"), a registered vendor on the MarketMinds Marketplace,

AND:
{BuyerName} ("Buyer"), a registered user on the MarketMinds Marketplace.

BUYER CONTACT DETAILS:
Full Name: {fullName}
Email: {email}
Phone Number: {phoneNumber}
Address: {address}
Postal Code: {postalCode}

PRODUCT DETAILS:
Description: {ProductDescription}
Purchase Price: ${Price}
Subtotal: ${subtotal}
Warranty Fee: ${warrantyTax}
Delivery Fee: ${deliveryFee}
Final Total: ${finalTotal}
Payment Method: {PaymentMethod}
Expected Delivery Date: {DeliveryDate}

1. PURCHASE AND SALE
   1.1 The Seller agrees to sell and the Buyer agrees to purchase the Product described above according to the terms and conditions outlined in this Agreement.
   1.2 The Buyer acknowledges having had the opportunity to inspect the Product''s description and specifications prior to purchase.

2. PAYMENT
   2.1 The Buyer agrees to pay the Final Total amount stated above.
   2.2 Payment has been processed through the MarketMinds payment system via the Payment Method indicated above.
   2.3 All prices are inclusive of applicable taxes unless otherwise specified.

3. DELIVERY
   3.1 The Seller shall arrange for delivery of the Product to the Buyer''s specified address.
   3.2 The expected delivery date is as specified above, subject to shipping conditions and availability.
   3.3 Risk of loss shall transfer to the Buyer upon delivery.

4. WARRANTY
   4.1 The Seller warrants that the Product is free from material defects and functions as advertised.
   4.2 This warranty is valid for 30 days from the date of delivery.
   4.3 The warranty does not cover damage resulting from misuse, accidents, or normal wear and tear.

5. RETURNS AND REFUNDS
   5.1 The Buyer may return the Product within 14 days of delivery if it does not meet the specifications advertised.
   5.2 Returns must be in original condition with all packaging and accessories.
   5.3 Refunds will be processed within 10 business days of the Seller receiving the returned Product.

6. LIMITATION OF LIABILITY
   6.1 The Seller''s liability is limited to the Purchase Price of the Product.
   6.2 Neither party shall be liable for indirect, special, or consequential damages.

7. GOVERNING LAW
   7.1 This Agreement shall be governed by and construed in accordance with the laws of the jurisdiction in which the Seller is based.

8. ADDITIONAL TERMS
   {AdditionalTerms}

The parties hereby indicate their acceptance of this Agreement by their signatures below or by their electronic acceptance through the MarketMinds platform.

Agreement Status: APPROVED

SELLER: {SellerName}
BUYER: {BuyerName}
DATE: {AgreementDate}'

SELECT * FROM PDFs

INSERT INTO PDFs
VALUES (CAST('Sample PDF content 1' AS VARBINARY(MAX))),
    (CAST('Sample PDF content 2' AS VARBINARY(MAX))),
    (CAST('Sample PDF content 3' AS VARBINARY(MAX)));

SELECT * FROM OrderHistory

INSERT INTO OrderHistory DEFAULT VALUES;
INSERT INTO OrderHistory DEFAULT VALUES;
INSERT INTO OrderHistory DEFAULT VALUES;

SELECT * FROM ProductCategories

INSERT INTO ProductCategories
VALUES ('Painting', 'Drawing stuff'),
	('Sculpture', 'idk bro')

SELECT * FROM Sellers
SELECT * FROM ProductConditions

INSERT INTO ProductConditions
VALUES ('good', 'kinda good'),
	('bad', 'kinda bad')

SELECT * FROM BuyProducts

SELECT * FROM Buyers

UPDATE Sellers
SET StoreName = 'Alexe Razvan'

UPDATE Buyers
SET FirstName = 'Barbos', LastName = 'Andrada'

GO
DECLARE @buyerID int;
SET @buyerID = 9
DECLARE @sellerID int;
SET @sellerID = 1
INSERT INTO BuyProducts (title, [description], price, stock, category_id, seller_id, condition_id)
VALUES
    ('Abstract Painting', 'Contemporary abstract art', 350.00, 1, 1, @sellerID, 1),
    ('Landscape Photo', 'Beautiful mountain landscape', 150.00, 5, 2, @sellerID, 1),
    ('Ceramic Vase', 'Handmade ceramic vase', 85.00, 8, 2, @sellerID, 1),
    ('Wooden Sculpture', 'Modern wooden sculpture', 275.00, 2, 2, @sellerID, 2),
    ('Digital Art Print', 'Limited edition digital art print', 125.00, 10, 1, @sellerID, 2),
    ('Metal Wall Art', 'Contemporary metal wall decoration', 225.00, 3, 2, @sellerID, 1);


SELECT * FROM OrderSummary
INSERT INTO OrderSummary
VALUES (10, 1, 2, 20, 'Full name johnny', 'johnny@gmail.com', '0712341234', 'Walls', 123, 'Add', 'Det')

UPDATE OrderSummary
SET Subtotal=350, WarrantyTax=3, DeliveryFee=7.99, FinalTotal=360.99, [Address]='Your walls', FullName='Alexe Adrian Gigel Constantin Razvan', Email='Alexis@gmail.com'

SELECT * FROM Orders
SELECT * FROM Buyers

INSERT INTO Orders (ProductID, [Name], BuyerID, SellerId, Cost, ProductType, PaymentMethod, OrderSummaryID, OrderDate, OrderHistoryID)
VALUES
    (1, 'Order1', @buyerID, @sellerID, 1, 'Digital', 'card', 1, '2024-03-15', 1),
    (2, 'Order2', @buyerID, @sellerID, 10, 'Type 0', 'wallet', 1, '2024-03-20', 2),
    (3, 'Order3', @buyerID, @sellerID, 100, 'Type sheet', 'cash', 1, '2024-03-25', 3);

SELECT * FROM Contracts
SELECT * FROM PDFs
SELECT * FROM PredefinedContracts
SELECT * FROM Orders

INSERT INTO Contracts (OrderID, ContractStatus, ContractContent, RenewalCount, PredefinedContractID, PDFID, AdditionalTerms)
VALUES (4, 'ACTIVE', 'This is a standard artwork purchase contract that outlines the terms and conditions. Please comply', 10, 3, 1, 'No additional terms'),
(5, 'ACTIVE', 'This is a standard artwork purchase contract that outlines the terms and conditions. Please comply', 10, 2, 1, 'No additional terms')


select * from BorrowProducts
SELECT * FROM BuyerCartItems
SELECT * FROM BuyerWishlistItems
