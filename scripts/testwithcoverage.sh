#!/bin/bash

# root of the project
root_dir=$(pwd)

# List of directories with test projects
directories=(
  "/test/NLightning.Bolts.Tests"
  "/test/NLightning.Common.Tests"
)

# Delete coverage directory
rm -rf $root_dir/coverage

# Loop over directories and run tests
for directory in "${directories[@]}"
do
  echo "Running tests in $directory"
  cd $root_dir/$directory
  # Add this when running Docker tests
  # export HOST_ADDRESS=$(ip route | awk 'NR==1 {print $3}')
  dotnet test --filter 'FullyQualifiedName!~Docker' --settings coverlet.runsettings --no-build --verbosity normal -l "console;verbosity=detailed" --collect:"XPlat Code Coverage" --logger "trx;LogFileName=test-results.trx" --results-directory $root_dir/coverage
done