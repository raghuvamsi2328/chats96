#!/usr/bin/env python3
"""
Simple script to test PostgreSQL database connection
"""
import psycopg2
import sys

def test_connection():
    # Connection parameters from your appsettings.Development.json
    connection_params = {
        'host': 'localhost',
        'port': 5432,
        'database': 'chatdb',
        'user': 'chatuser',
        'password': 'chatpassword'
    }
    
    try:
        print("Attempting to connect to PostgreSQL database...")
        print(f"Host: {connection_params['host']}")
        print(f"Port: {connection_params['port']}")
        print(f"Database: {connection_params['database']}")
        print(f"User: {connection_params['user']}")
        print()
        
        # Attempt connection
        conn = psycopg2.connect(**connection_params)
        cursor = conn.cursor()
        
        # Test basic query
        cursor.execute("SELECT version();")
        version = cursor.fetchone()
        
        print("✅ Connection successful!")
        print(f"PostgreSQL version: {version[0]}")
        
        # Test if your tables exist
        cursor.execute("""
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = 'public' AND table_type = 'BASE TABLE';
        """)
        tables = cursor.fetchall()
        
        if tables:
            print(f"\n📋 Existing tables:")
            for table in tables:
                print(f"  - {table[0]}")
        else:
            print(f"\n⚠️  No tables found - database needs migration")
        
        cursor.close()
        conn.close()
        
    except psycopg2.Error as e:
        print("❌ Database connection failed!")
        print(f"Error code: {e.pgcode}")
        print(f"Error message: {e.pgerror}")
        print()
        
        if "could not connect to server" in str(e):
            print("💡 Suggestions:")
            print("  1. Make sure PostgreSQL is running")
            print("  2. Check if Docker containers are up: docker-compose ps")
            print("  3. Start containers: docker-compose up -d database")
            
        elif "authentication failed" in str(e):
            print("💡 Suggestions:")
            print("  1. Check username/password in connection string")
            print("  2. Verify database user permissions")
            
        elif "database" in str(e) and "does not exist" in str(e):
            print("💡 Suggestions:")
            print("  1. Create the database: docker-compose up -d database")
            print("  2. Check database name in connection string")
        
    except Exception as e:
        print(f"❌ Unexpected error: {e}")
        print("\n💡 Make sure psycopg2 is installed: pip install psycopg2-binary")

if __name__ == "__main__":
    test_connection()
