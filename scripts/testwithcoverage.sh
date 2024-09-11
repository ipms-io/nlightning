#!/bin/bash

# config
config=${1:-"Release"}

# root of the project
root_dir=$(pwd)

# List of directories with test projects
directories=(
  "/test/NLightning.Bolts.Tests"
  "/test/NLightning.Common.Tests"
)

# Delete coverage directory
rm -rf "$root_dir"/coverage

# Initialize a flag to capture any test failure
any_fail=0

# Loop over directories and run tests
for directory in "${directories[@]}"
do
  echo "Running tests in $directory"
  cd "$root_dir"/"$directory" || exit
  # Add this when running Docker tests
  # export HOST_ADDRESS=$(ip route | awk 'NR==1 {print $3}')
  dotnet test -c "$config" --filter 'FullyQualifiedName!~Docker' --settings coverlet.runsettings --no-build --verbosity normal -l "console;verbosity=detailed" --collect:"XPlat Code Coverage" --logger "trx;LogFileName=test-results.trx" --results-directory $root_dir/coverage
  
  # Capture the exit code
  exit_code=$?
  
  # Check if the test run was successful
  if [ $exit_code -ne 0 ]; then
    echo "Tests failed in $directory"
    any_fail=1
  fi
done

# Exit with an error if any tests failed
if [ $any_fail -ne 0 ]; then
  echo "Some tests failed. Exiting with status 1."
  exit 1
fi

echo "All tests passed successfully."