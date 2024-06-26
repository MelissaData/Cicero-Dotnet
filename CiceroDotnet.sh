#!/bin/bash

# Name:    CiceroCloudAPI
# Purpose: Execute the CiceroCloudAPI program

######################### Constants ##########################

RED='\033[0;31m' #RED
NC='\033[0m' # No Color

######################### Parameters ##########################

lat=""
long=""
location=""
max=""
license=""

while [ $# -gt 0 ] ; do
  case $1 in
    --lat)
        if [ -z "$2" ] || [[ $2 =~ ^--?[a-zA-Z]+$ ]];
        then
            printf "${RED}Error: Missing an argument for parameter 'lat'.${NC}\n"
            exit 1
        fi
        lat="$2"
        shift
        ;;
    --long)
        if [ -z "$2" ] || [[ $2 =~ ^--?[a-zA-Z]+$ ]];
        then
            printf "${RED}Error: Missing an argument for parameter 'long'.${NC}\n"
            exit 1
        fi
        long="$2"
        shift
        ;;
    --location)
        if [ -z "$2" ] || [[ $2 =~ ^--?[a-zA-Z]+$ ]];
        then
            printf "${RED}Error: Missing an argument for parameter 'location'.${NC}\n"
            exit 1
        fi
        location="$2"
        shift
        ;;
    --max)
        if [ -z "$2" ] || [[ $2 =~ ^--?[a-zA-Z]+$ ]];
        then
            printf "${RED}Error: Missing an argument for parameter 'max'.${NC}\n"
            exit 1
        fi
        max="$2"
        shift
        ;;
    --license)
        if [ -z "$2" ] || [[ $2 =~ ^--?[a-zA-Z]+$ ]];
        then
            printf "${RED}Error: Missing an argument for parameter 'license'.${NC}\n"
            exit 1
        fi
        license="$2"
        shift
        ;;
  esac
  shift
done


# Use the location of the .sh file
# Modify this if you want to use
CurrentPath="$(pwd)"
ProjectPath="$CurrentPath/CiceroDotnet"
BuildPath="$ProjectPath/Build"

if [ ! -d "$BuildPath" ];
then
    mkdir "$BuildPath"
fi

########################## Main ############################
printf "\n=================================== Melissa Cicero Cloud API =====================================\n"

# Get license (either from parameters or user input)
if [ -z "$license" ];
then
  printf "Please enter your license string: "
  read license
fi

# Check for License from Environment Variables 
if [ -z "$license" ];
then
  license=`echo $MD_LICENSE` 
fi

if [ -z "$license" ];
then
  printf "\nLicense String is invalid!\n"
  exit 1
fi

# Start program
# Build project
printf "\n=============================== BUILD PROJECT ==============================\n"

dotnet publish -f="net7.0" -c Release -o "$BuildPath" CiceroDotnet/CiceroDotnet.csproj

# Run project
if [ -z "$lat" ] && [ -z "$long" ];
then
    dotnet "$BuildPath"/CiceroDotnet.dll --license $license 
else
    dotnet "$BuildPath"/CiceroDotnet.dll --license $license --lat "$lat" --long "$long" --location "$location" --max "$max"
fi

