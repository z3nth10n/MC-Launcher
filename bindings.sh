#!/bin/bash
rm -rf z3nth10n-PHP/
cp -r /c/xampp/htdocs/z3nth10n-PHP/ z3nth10n-PHP/
read -p "Commit message: " commit_msg
git -C z3nth10n-PHP/ add .
git -C z3nth10n-PHP/ commit -m $commit_msg
git -C z3nth10n-PHP/ push -u origin master
