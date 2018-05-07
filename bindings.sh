#!/bin/bash
rm -rf z3nth10n-PHP/
cp -r /c/xampp/htdocs/z3nth10n-PHP/ z3nth10n-PHP/
fpath="$(pwd)/z3nth10n-PHP/"
read -p "Commit message: " commit_msg
git -C "$fpath" add --all
git -C "$fpath" commit -m "$commit_msg"
git -C "$fpath" push -u origin master
