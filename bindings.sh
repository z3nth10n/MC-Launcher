#!/bin/bash
fpath="$(pwd)/z3nth10n-PHP/"
yes | cp -rf /c/xampp/htdocs/z3nth10n-PHP/ "$fpath"
fpath+=".git"
read -p "Commit message: " commit_msg
git --git-dir="$fpath" add --all
git --git-dir="$fpath" commit -m "$commit_msg"
git --git-dir="$fpath" push -u origin master

read -p "Commit message for this repo: " commit_msg
git add --all
git commit -m "$commit_msg"
git push -u origin master