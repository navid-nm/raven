import subprocess
import shutil
import os

# Define paths
source_path = r"A:\Projects\Apps\Raven\src\bin\Release\net8.0\win-x64\publish\raven.exe"
destination_path = r"A:\Scripts\Raven.exe"

# Function to run the dotnet publish command
def run_dotnet_publish():
    try:
        print("Running dotnet publish")
        # Run the dotnet publish command
        result = subprocess.run(["dotnet", "publish"], check=True, text=True, capture_output=True)
        print("Dotnet publish output:", result.stdout)
    except subprocess.CalledProcessError as e:
        print("Error running dotnet publish:", e.stderr)
        raise

# Function to move the file
def move_file():
    if os.path.exists(source_path):
        try:
            # Move the file
            shutil.move(source_path, destination_path)
            print(f"File moved to {destination_path}")
        except Exception as e:
            print(f"Error moving file: {e}")
            raise
    else:
        print(f"Source file {source_path} does not exist.")

# Main script execution
if __name__ == "__main__":
    run_dotnet_publish()
    move_file()
