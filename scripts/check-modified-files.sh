#!/bin/bash

shopt -s globstar

# Check if an argument is passed, if not exit with an error
if [ $# -eq 0 ]; then
  echo "Usage: $0 path/to/your.csproj"
  exit 1
fi

# First argument is the .csproj file location
CS_PROJECT_FILE="$1"

# Extract the directory path of the .csproj file
CS_PROJECT_DIR=$(dirname "$CS_PROJECT_FILE")
echo "Checking for changes in files under $CS_PROJECT_DIR"

# Get the latest bolt11 tag matching the pattern "bolt11-v*"
LAST_RELEASE_TAG=$(git tag -l 'bolt11-v*' --sort=-v:refname | head -n 1)

# Fetch the commit hash for the latest tag
LATEST_TAG_COMMIT=$(git rev-list -n 1 "$LAST_RELEASE_TAG" --)

# Function to extract property values
extract_property() {
    grep "<$1>" "$CS_PROJECT_FILE" | sed -E "s/.*<$1>(.*)<\/$1>.*/\1/"
}

echo "Checking changes since tag $LAST_RELEASE_TAG ($LATEST_TAG_COMMIT)"

# Expand wildcards and get a list of actual files
FILE_PATHS=$(awk -F'"' '/<Compile Include=/ {print $2}' "$CS_PROJECT_FILE")
declare -a FILES_TO_CHECK=()
for path in $FILE_PATHS; do
  # Prepend the .csproj directory to the relative paths
  FULL_PATH="$CS_PROJECT_DIR/${path//\\//}"

  for file in $FULL_PATH; do
    FILES_TO_CHECK+=("$(echo $file)")
  done
done

# Check if any of these files have changed since the last tag
CHANGED_FILES=$(git diff --name-only "$LATEST_TAG_COMMIT" -- "${FILES_TO_CHECK[@]}")
if [ -n "$CHANGED_FILES" ]; then
  echo "Changes detected in watched files since the last release."
  
  # Extract values from the current .csproj file
  current_version=$(extract_property "Version")
  current_assembly_version=$(extract_property "AssemblyVersion")
  current_file_version=$(extract_property "FileVersion")

  # Extract values from the previous commit
  git checkout $PREV_COMMIT_HASH -- "$CS_PROJECT_FILE" > /dev/null 2>&1
  previous_version=$(extract_property "Version")
  previous_assembly_version=$(extract_property "AssemblyVersion")
  previous_file_version=$(extract_property "FileVersion")

  # Checkout back to the last commit
  git checkout $LAST_COMMIT_HASH > /dev/null 2>&1

  # Check for changes
  changes=0
  if [[ "$current_version" != "$previous_version" ]]; then
      echo "Version changed from $previous_version to $current_version"
      changes+=1
  fi

  if [[ "$current_assembly_version" != "$previous_assembly_version" ]]; then
      echo "AssemblyVersion changed from $previous_assembly_version to $current_assembly_version"
      changes+=1
  fi

  if [[ "$current_file_version" != "$previous_file_version" ]]; then
      echo "FileVersion changed from $previous_file_version to $current_file_version"
      changes+=1
  fi

  # Set action output if using GitHub Actions
  if [[ $changes -eq 3 ]]; then
    echo "All version properties have changed. You can publish a new release."
    echo "::set-output name=publish::true"
  else
    echo "You have to bump Version, AssemblyVersion, and FileVersion to publish a new release."
    echo "::set-output name=publish::false"
    exit 1
  fi
else
  echo "No changes detected in watched files since the last release."
  echo "::set-output name=publish::false"
fi