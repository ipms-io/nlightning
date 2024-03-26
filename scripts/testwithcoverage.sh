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
  dotnet test --settings coverlet.runsettings --collect:"XPlat Code Coverage" --no-build --verbosity normal --results-directory $root_dir/coverage -l "console;verbosity=detailed" --logger "trx;LogFileName=test-results.trx"
done