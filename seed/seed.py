import json
import random
import sys

def get_condition_id(exterior, item_type):
    """Map CS:GO exterior/type to database condition ID"""
    condition_mapping = {
        "Factory New": 1,
        "Minimal Wear": 11,
        "Field-Tested": 12,
        "Well-Worn": 13,
        "Battle-Scarred": 14,
        "Souvenir": 15
    }
    
    # If it's a souvenir item, prioritize souvenir condition
    if item_type == "Souvenir":
        return 15
    
    return condition_mapping.get(exterior, 9)  # Default to "Used" if not found

def generate_price(rarity, exterior):
    """Generate realistic price based on rarity and exterior"""
    base_prices = {
        "Consumer Grade": (5, 25),
        "Industrial Grade": (10, 50),
        "Mil-Spec": (25, 100),
        "Restricted": (50, 300),
        "Classified": (100, 800),
        "Covert": (500, 5000),
        "Contraband": (2000, 15000)
    }
    
    # Exterior multipliers
    exterior_multipliers = {
        "Factory New": 1.0,
        "Minimal Wear": 0.85,
        "Field-Tested": 0.7,
        "Well-Worn": 0.5,
        "Battle-Scarred": 0.3
    }
    
    rarity = rarity or "Consumer Grade"
    min_price, max_price = base_prices.get(rarity, (10, 100))
    multiplier = exterior_multipliers.get(exterior, 0.7)
    
    base_price = random.uniform(min_price, max_price)
    final_price = base_price * multiplier
    
    return round(final_price, 2)

def generate_stock():
    """Generate random stock between 1-20"""
    return random.randint(1, 20)

def create_description(item_data):
    """Create description from item data"""
    weapon = item_data.get('weapon', '')
    finish = item_data.get('finish', '')
    exterior = item_data.get('exterior', '')
    rarity = item_data.get('rarity', '')
    finish_style = item_data.get('finish-style', '')
    item_type = item_data.get('type', '')
    
    description_parts = []
    
    if item_type == "Souvenir":
        description_parts.append("Rare souvenir")
    
    description_parts.append(f"{weapon} skin")
    
    if finish:
        description_parts.append(f"with {finish} finish")
    
    if finish_style:
        description_parts.append(f"featuring {finish_style.lower()}")
    
    if exterior:
        description_parts.append(f"in {exterior.lower()} condition")
    
    if rarity:
        description_parts.append(f"({rarity} rarity)")
    
    description_parts.append("Perfect for CS:GO collectors and players.")
    
    return " ".join(description_parts).replace("  ", " ")

def escape_sql_string(text):
    """Escape single quotes in SQL strings"""
    return text.replace("'", "''")

def generate_sql_inserts(json_data, batch_size=5000):
    """Generate SQL insert statements from JSON data in batches"""
    all_products = []
    
    # Prepare all product data first
    for item_name, item_data in json_data.items():
        title = escape_sql_string(item_data.get('full-name', item_name))
        description = escape_sql_string(create_description(item_data))
        image_url = item_data.get('image', '')
        exterior = item_data.get('exterior', '')
        item_type = item_data.get('type', '')
        rarity = item_data.get('rarity', '')
        
        condition_id = get_condition_id(exterior, item_type)
        price = generate_price(rarity, exterior)
        stock = generate_stock()
        
        all_products.append({
            'title': title,
            'description': description,
            'image_url': image_url,
            'condition_id': condition_id,
            'price': price,
            'stock': stock
        })
    
    # Generate SQL in batches
    sql_batches = []
    
    for i in range(0, len(all_products), batch_size):
        batch = all_products[i:i + batch_size]
        batch_sql = generate_batch_sql(batch, i + 1)
        sql_batches.append(batch_sql)
    
    return sql_batches

def generate_batch_sql(products_batch, start_index):
    """Generate SQL for a batch of products using bulk insert approach"""
    
    # Create temporary table and bulk insert
    sql_parts = []
    
    sql_parts.append(f"""-- Batch starting from product {start_index}
-- Create temporary table for bulk operations
CREATE TABLE #TempProducts (
    TempId INT IDENTITY(1,1),
    Title NVARCHAR(500),
    Description NVARCHAR(2000),
    ImageUrl NVARCHAR(1000),
    ConditionId INT,
    Price DECIMAL(10,2),
    Stock INT
);

-- Bulk insert products
""")
    
    # Add bulk insert values
    values_list = []
    for product in products_batch:
        values_list.append(f"('{product['title']}', '{product['description']}', '{product['image_url']}', {product['condition_id']}, {product['price']}, {product['stock']})")
    
    # Split into smaller chunks to avoid SQL length limits
    chunk_size = 1000
    for i in range(0, len(values_list), chunk_size):
        chunk = values_list[i:i + chunk_size]
        sql_parts.append(f"""INSERT INTO #TempProducts (Title, Description, ImageUrl, ConditionId, Price, Stock) VALUES
{',\n'.join(chunk)};

""")
    
    # Insert into actual tables using the temp table
    sql_parts.append("""-- Insert into BuyProducts from temp table
INSERT INTO BuyProducts (title, description, seller_id, condition_id, category_id, price, stock)
SELECT Title, Description, 1, ConditionId, 1, Price, Stock
FROM #TempProducts;

-- Get the range of inserted product IDs
DECLARE @FirstProductId INT = SCOPE_IDENTITY() - @@ROWCOUNT + 1;
DECLARE @LastProductId INT = SCOPE_IDENTITY();

-- Insert images using a loop (more efficient than individual variables)
DECLARE @CurrentId INT = @FirstProductId;
DECLARE @TempId INT = 1;

WHILE @CurrentId <= @LastProductId
BEGIN
    INSERT INTO BuyProductImages (product_id, url)
    SELECT @CurrentId, ImageUrl
    FROM #TempProducts
    WHERE TempId = @TempId;
    
    -- Insert tags
    INSERT INTO BuyProductProductTags (product_id, tag_id) VALUES 
    (@CurrentId, 33), -- collectible
    (@CurrentId, 32); -- gift
    
    SET @CurrentId = @CurrentId + 1;
    SET @TempId = @TempId + 1;
END;

-- Clean up
DROP TABLE #TempProducts;

""")
    
    return "\n".join(sql_parts)

def main():
    if len(sys.argv) != 2:
        print("Usage: python csgo_to_sql.py <json_file_path>")
        sys.exit(1)
    
    json_file_path = sys.argv[1]
    
    try:
        with open(json_file_path, 'r', encoding='utf-8') as file:
            json_data = json.load(file)
        
        print(f"Processing {len(json_data)} items...")
        sql_batches = generate_sql_inserts(json_data, batch_size=5000)
        
        # Write each batch to a separate file
        for i, batch_sql in enumerate(sql_batches):
            output_file = json_file_path.replace('.json', f'_buy_products_batch_{i+1}.sql')
            with open(output_file, 'w', encoding='utf-8') as file:
                file.write(batch_sql)
            print(f"Generated batch {i+1}: {output_file}")
        
        print(f"Successfully generated {len(sql_batches)} SQL batch files")
        print(f"Total products: {len(json_data)}")
        
    except FileNotFoundError:
        print(f"Error: File '{json_file_path}' not found")
    except json.JSONDecodeError:
        print(f"Error: Invalid JSON in file '{json_file_path}'")
    except Exception as e:
        print(f"Error: {str(e)}")

if __name__ == "__main__":
    main()
