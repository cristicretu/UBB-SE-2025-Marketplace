import json
import random
import sys
import os

def get_condition_id(exterior=None, item_type=None, is_minecraft=False):
    """Map CS:GO exterior/type to database condition ID or random for Minecraft"""
    condition_mapping = {
        "Factory New": 1,
        "Minimal Wear": 11,
        "Field Tested": 12,  # Fixed: removed hyphen
        "Well Worn": 13,     # Fixed: removed hyphen
        "Battle Scarred": 14, # Fixed: removed hyphen
        "Souvenir": 15,
        # Also handle hyphenated versions from CS:GO data
        "Field-Tested": 12,
        "Well-Worn": 13,
        "Battle-Scarred": 14
    }
    
    if is_minecraft:
        # Random condition for Minecraft items (all 15 conditions)
        return random.randint(1, 15)
    
    # If it's a souvenir item, prioritize souvenir condition
    if item_type == "Souvenir":
        return 15
    
    return condition_mapping.get(exterior, 9)  # Default to "Used" if not found

def get_random_category_id():
    """Get random category ID (1-14)"""
    return random.randint(1, 14)

def get_random_tags():
    """Get 2-3 random tag IDs"""
    num_tags = random.randint(2, 3)
    return random.sample(range(1, 26), num_tags)

def generate_price(rarity=None, exterior=None, is_minecraft=False):
    """Generate realistic price based on rarity and exterior or random for Minecraft"""
    if is_minecraft:
        # Random price for Minecraft items
        return round(random.uniform(5, 500), 2)
    
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

def create_description_csgo(item_data):
    """Create description from CS:GO item data"""
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

def create_description_minecraft(item_data):
    """Create description from Minecraft item data"""
    description = item_data.get('description', '')
    stack_size = item_data.get('stackSize', 1)
    
    if description:
        description += f" Stack size: {stack_size}. Perfect for Minecraft builders and adventurers."
    else:
        description = f"Minecraft item with stack size {stack_size}. Great for your next adventure!"
    
    return description

def escape_sql_string(text):
    """Escape single quotes in SQL strings"""
    if not text:
        return ""
    return str(text).replace("'", "''")

def prepare_csgo_products(json_data):
    """Prepare CS:GO products data"""
    products = []
    
    for item_name, item_data in json_data.items():
        title = escape_sql_string(item_data.get('full-name', item_name))
        description = escape_sql_string(create_description_csgo(item_data))
        image_url = item_data.get('image', '')
        exterior = item_data.get('exterior', '')
        item_type = item_data.get('type', '')
        rarity = item_data.get('rarity', '')
        
        condition_id = get_condition_id(exterior, item_type, False)
        price = generate_price(rarity, exterior, False)
        stock = generate_stock()
        category_id = get_random_category_id()
        tags = get_random_tags()
        
        products.append({
            'title': title,
            'description': description,
            'image_url': image_url,
            'condition_id': condition_id,
            'price': price,
            'stock': stock,
            'category_id': category_id,
            'tags': tags,
            'source': 'csgo'
        })
    
    return products

def prepare_minecraft_products(json_data):
    """Prepare Minecraft products data"""
    products = []
    
    # Handle both list and dict formats
    if isinstance(json_data, list):
        # If it's a list, iterate through items directly
        for item_data in json_data:
            item_name = item_data.get('name', 'Unknown Item')
            title = escape_sql_string(item_name)
            description = escape_sql_string(create_description_minecraft(item_data))
            image_url = item_data.get('image', '')
            
            condition_id = get_condition_id(is_minecraft=True)
            price = generate_price(is_minecraft=True)
            stock = generate_stock()
            category_id = get_random_category_id()
            tags = get_random_tags()
            
            products.append({
                'title': title,
                'description': description,
                'image_url': image_url,
                'condition_id': condition_id,
                'price': price,
                'stock': stock,
                'category_id': category_id,
                'tags': tags,
                'source': 'minecraft'
            })
    else:
        # If it's a dict, iterate through key-value pairs
        for item_name, item_data in json_data.items():
            title = escape_sql_string(item_data.get('name', item_name))
            description = escape_sql_string(create_description_minecraft(item_data))
            image_url = item_data.get('image', '')
            
            condition_id = get_condition_id(is_minecraft=True)
            price = generate_price(is_minecraft=True)
            stock = generate_stock()
            category_id = get_random_category_id()
            tags = get_random_tags()
            
            products.append({
                'title': title,
                'description': description,
                'image_url': image_url,
                'condition_id': condition_id,
                'price': price,
                'stock': stock,
                'category_id': category_id,
                'tags': tags,
                'source': 'minecraft'
            })
    
    return products

def generate_batch_sql(products_batch, start_index, product_type):
    """Generate SQL for a batch of products for specific product type (buy/borrow/auction)"""
    
    table_mapping = {
        'buy': {
            'product_table': 'BuyProducts',
            'image_table': 'BuyProductImages',
            'tag_table': 'BuyProductProductTags'
        },
        'borrow': {
            'product_table': 'BorrowProducts',
            'image_table': 'BorrowProductImages', 
            'tag_table': 'BorrowProductProductTags'
        },
        'auction': {
            'product_table': 'AuctionProducts',
            'image_table': 'AuctionProductsImages',  # Correct: matches ProductImage model
            'tag_table': 'AuctionProductProductTags'
        }
    }
    
    tables = table_mapping[product_type]
    sql_parts = []
    
    # Different temp table structure based on product type
    if product_type == 'buy':
        temp_table_sql = """CREATE TABLE #TempProducts (
    TempId INT IDENTITY(1,1),
    Title NVARCHAR(500),
    Description NVARCHAR(2000),
    ImageUrl NVARCHAR(1000),
    ConditionId INT,
    CategoryId INT,
    Price DECIMAL(10,2),
    Stock INT,
    Tags NVARCHAR(100)
);"""
    elif product_type == 'borrow':
        temp_table_sql = """CREATE TABLE #TempProducts (
    TempId INT IDENTITY(1,1),
    Title NVARCHAR(500),
    Description NVARCHAR(2000),
    ImageUrl NVARCHAR(1000),
    ConditionId INT,
    CategoryId INT,
    DailyRate DECIMAL(10,2),
    Stock INT,
    Tags NVARCHAR(100)
);"""
    else:  # auction
        temp_table_sql = """CREATE TABLE #TempProducts (
    TempId INT IDENTITY(1,1),
    Title NVARCHAR(500),
    Description NVARCHAR(2000),
    ImageUrl NVARCHAR(1000),
    ConditionId INT,
    CategoryId INT,
    StartingPrice DECIMAL(10,2),
    Stock INT,
    Tags NVARCHAR(100)
);"""
    
    sql_parts.append(f"""-- {product_type.title()} Products Batch starting from product {start_index}
-- Create temporary table for bulk operations
{temp_table_sql}

-- Bulk insert products
""")
    
    # Add bulk insert values with different column names
    values_list = []
    for product in products_batch:
        tags_str = ','.join(map(str, product['tags']))
        if product_type == 'buy':
            values_list.append(f"('{product['title']}', '{product['description']}', '{product['image_url']}', {product['condition_id']}, {product['category_id']}, {product['price']}, {product['stock']}, '{tags_str}')")
        elif product_type == 'borrow':
            values_list.append(f"('{product['title']}', '{product['description']}', '{product['image_url']}', {product['condition_id']}, {product['category_id']}, {product['price']}, {product['stock']}, '{tags_str}')")
        else:  # auction
            values_list.append(f"('{product['title']}', '{product['description']}', '{product['image_url']}', {product['condition_id']}, {product['category_id']}, {product['price']}, {product['stock']}, '{tags_str}')")
    
    # Split into smaller chunks to avoid SQL length limits
    chunk_size = 1000
    for i in range(0, len(values_list), chunk_size):
        chunk = values_list[i:i + chunk_size]
        if product_type == 'buy':
            insert_columns = "(Title, Description, ImageUrl, ConditionId, CategoryId, Price, Stock, Tags)"
        elif product_type == 'borrow':
            insert_columns = "(Title, Description, ImageUrl, ConditionId, CategoryId, DailyRate, Stock, Tags)"
        else:  # auction
            insert_columns = "(Title, Description, ImageUrl, ConditionId, CategoryId, StartingPrice, Stock, Tags)"
            
        sql_parts.append(f"""INSERT INTO #TempProducts {insert_columns} VALUES
{',\n'.join(chunk)};

""")
    
    # Generate product-specific insert based on type
    if product_type == 'buy':
        product_insert = "INSERT INTO BuyProducts (title, description, seller_id, condition_id, category_id, price, stock)"
        select_columns = "Title, Description, ABS(CHECKSUM(NEWID())) % 5 + 1, ConditionId, CategoryId, Price, Stock"
    elif product_type == 'borrow':
        product_insert = "INSERT INTO BorrowProducts (title, description, seller_id, condition_id, category_id, daily_rate, stock, time_limit, start_date, end_date, is_borrowed)"
        select_columns = """Title, Description, 
       ABS(CHECKSUM(NEWID())) % 5 + 1, -- Random seller_id between 1-5
       ConditionId, CategoryId, DailyRate, Stock,
       DATEADD(day, 30, GETDATE()), -- time_limit (30 days from now)
       GETDATE(), -- start_date (current date)
       DATEADD(day, 30, GETDATE()), -- end_date (30 days from now)
       0"""   # is_borrowed (false)
    else:  # auction
        product_insert = "INSERT INTO AuctionProducts (title, description, seller_id, condition_id, category_id, starting_price, current_price, price, stock, start_datetime, end_datetime)"
        select_columns = """Title, Description,
       ABS(CHECKSUM(NEWID())) % 5 + 1, -- Random seller_id between 1-5
       ConditionId, CategoryId, StartingPrice, StartingPrice, StartingPrice, Stock,
       DATEADD(hour, ABS(CHECKSUM(NEWID())) % 168, GETDATE()), -- start_datetime (random within next week)
       DATEADD(day, 7 + ABS(CHECKSUM(NEWID())) % 14, GETDATE())"""  # end_datetime (1-3 weeks from now)
    
    # Insert into actual tables using the temp table
    sql_parts.append(f"""-- Insert into {tables['product_table']} from temp table
{product_insert}
SELECT {select_columns}
FROM #TempProducts;

-- Get the range of inserted product IDs
DECLARE @FirstProductId INT = SCOPE_IDENTITY() - @@ROWCOUNT + 1;
DECLARE @LastProductId INT = SCOPE_IDENTITY();

-- Insert images and tags using a loop
DECLARE @CurrentId INT = @FirstProductId;
DECLARE @TempId INT = 1;

WHILE @CurrentId <= @LastProductId
BEGIN
    -- Insert image
    INSERT INTO {tables['image_table']} (product_id, url)
    SELECT @CurrentId, ImageUrl
    FROM #TempProducts
    WHERE TempId = @TempId AND ImageUrl != '';
    
    -- Insert tags
    DECLARE @Tags NVARCHAR(100);
    SELECT @Tags = Tags FROM #TempProducts WHERE TempId = @TempId;
    
    DECLARE @TagId INT;
    DECLARE @Pos INT = 1;
    DECLARE @NextPos INT;
    
    WHILE @Pos <= LEN(@Tags)
    BEGIN
        SET @NextPos = CHARINDEX(',', @Tags, @Pos);
        IF @NextPos = 0 SET @NextPos = LEN(@Tags) + 1;
        
        SET @TagId = CAST(SUBSTRING(@Tags, @Pos, @NextPos - @Pos) AS INT);
        -- Insert tag for {product_type} product
        INSERT INTO {tables['tag_table']} ({('ProductId, TagId' if product_type == 'auction' else 'product_id, tag_id')}) VALUES (@CurrentId, @TagId);
        
        SET @Pos = @NextPos + 1;
    END;
    
    SET @CurrentId = @CurrentId + 1;
    SET @TempId = @TempId + 1;
END;

-- Clean up
DROP TABLE #TempProducts;

""")
    
    return "\n".join(sql_parts)

def generate_sql_inserts(all_products, product_type, batch_size=5000):
    """Generate SQL insert statements for specific product type in batches"""
    sql_batches = []
    
    for i in range(0, len(all_products), batch_size):
        batch = all_products[i:i + batch_size]
        batch_sql = generate_batch_sql(batch, i + 1, product_type)
        sql_batches.append(batch_sql)
    
    return sql_batches

def main():
    if len(sys.argv) < 2:
        print("Usage: python seed.py <csgo_json_file> [minecraft_json_file]")
        sys.exit(1)
    
    csgo_file = sys.argv[1]
    minecraft_file = sys.argv[2] if len(sys.argv) > 2 else None
    
    try:
        all_products = []
        
        # Load CS:GO data
        if os.path.exists(csgo_file):
            with open(csgo_file, 'r', encoding='utf-8') as file:
                csgo_data = json.load(file)
            print(f"Processing {len(csgo_data)} CS:GO items...")
            all_products.extend(prepare_csgo_products(csgo_data))
        
        # Load Minecraft data if provided
        if minecraft_file and os.path.exists(minecraft_file):
            with open(minecraft_file, 'r', encoding='utf-8') as file:
                minecraft_data = json.load(file)
            print(f"Processing {len(minecraft_data)} Minecraft items...")
            all_products.extend(prepare_minecraft_products(minecraft_data))
        
        if not all_products:
            print("No products to process!")
            sys.exit(1)
        
        # Shuffle products for better distribution
        random.shuffle(all_products)
        
        # Generate SQL for each product type
        product_types = ['buy', 'borrow', 'auction']
        
        for product_type in product_types:
            print(f"\nGenerating {product_type} products...")
            sql_batches = generate_sql_inserts(all_products, product_type, batch_size=5000)
            
            # Write each batch to a separate file
            for i, batch_sql in enumerate(sql_batches):
                base_name = os.path.splitext(csgo_file)[0]
                output_file = f"{base_name}_{product_type}_products_batch_{i+1}.sql"
                with open(output_file, 'w', encoding='utf-8') as file:
                    file.write(batch_sql)
                print(f"Generated {product_type} batch {i+1}: {output_file}")
        
        print(f"\nSuccessfully generated SQL files for all product types")
        print(f"Total products per type: {len(all_products)}")
        print(f"CS:GO products: {len([p for p in all_products if p['source'] == 'csgo'])}")
        print(f"Minecraft products: {len([p for p in all_products if p['source'] == 'minecraft'])}")
        
    except FileNotFoundError as e:
        print(f"Error: File not found - {str(e)}")
    except json.JSONDecodeError as e:
        print(f"Error: Invalid JSON - {str(e)}")
    except Exception as e:
        print(f"Error: {str(e)}")

if __name__ == "__main__":
    main()
