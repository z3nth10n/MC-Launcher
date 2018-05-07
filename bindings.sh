#!/bin/bash
rm -rf z3nth10n-PHP/
cp -r /c/xampp/htdocs/z3nth10n-PHP/ z3nth10n-PHP/
cd z3nth10n-PHP/
read -p "Commit message: " commit_msg
git add .
git commit -m $commit_msg
git push -u origin master
cd ..
