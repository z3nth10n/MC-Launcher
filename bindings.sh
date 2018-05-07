#!/bin/bash
fpath="z3nth10n-PHP"
htdocs="/c/xampp/htdocs/z3nth10n-PHP"

function php_commit 
{
  for file in $(find "$fpath" -type f | grep -wv ".git"); do
    rm -rf "$file"
  done

  cp -rf "$htdocs/." z3nth10n-PHP/.

  read -p "[PHP] Commit message: " commit_msg

  if [[ ! -z `git -C "$fpath" diff HEAD .gitignore` ]]; then
    git -C "$fpath" rm -rf --cached .
  fi

  git -C "$fpath" add --all
  git -C "$fpath" commit -m "$commit_msg"
  git -C "$fpath" push -u origin master
}

function main_commit 
{
  read -p "[Main] Commit message: " commit_msg

  if [[ ! -z `git diff HEAD .gitignore` ]]; then
    git -C "$fpath" rm -rf --cached .
  fi

  git add --all
  git commit -m "$commit_msg"
  git push -u origin master
}

echo ""
echo "Please select an option:"
echo "  0) Only PHP commit"
echo "  1) Only main commit"
echo "  2) Both commits"
echo ""
read -p "Select an option: " opt

case $opt in
"0")
  php_commit
  ;;
"1")
  main_commit
  ;;
"2")
  php_commit
  main_commit
  ;;
*)
  echo "Unrecognized option, call 'git bind' again."
  ;;
esac