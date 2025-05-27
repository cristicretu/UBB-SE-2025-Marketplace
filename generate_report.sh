#!/bin/bash

# Configuration
SINCE_DATE="2024-05-12"
OUTPUT_FILE="contribution_report.txt"

# Start fresh
echo "Team Contribution Report (Since $SINCE_DATE)" > $OUTPUT_FILE
echo "===========================================" >> $OUTPUT_FILE
echo "" >> $OUTPUT_FILE

# Get list of authors properly (handles spaces correctly)
readarray -t authors < <(git log --all --since="$SINCE_DATE" --format='%aN' | sort | uniq)

# Get list of all branches (local + remote)
branches=$(git for-each-ref --format='%(refname:short)' refs/heads refs/remotes)

# Loop properly through each author
for author in "${authors[@]}"; do
    echo "Processing author: $author"

    echo "-------------------------------------------" >> $OUTPUT_FILE
    echo "Author: $author" >> $OUTPUT_FILE

    # Number of commits
    commit_count=$(git log --all --since="$SINCE_DATE" --author="$author" --pretty=oneline | wc -l)
    echo "Number of Git Commits: $commit_count" >> $OUTPUT_FILE

    # Branches used by author
    author_branches=""
    for branch in $branches; do
        # Skip weird refs like origin/HEAD or empty
        if [[ "$branch" == *"HEAD"* ]] || [[ -z "$branch" ]]; then
            continue
        fi
        # Add -- after branch name to avoid folder/file confusion
        if git log "$branch" --since="$SINCE_DATE" --author="$author" --pretty=oneline -- | grep . >/dev/null; then
            author_branches+="$branch"$'\n'
        fi
    done

    echo "Branches Used:" >> $OUTPUT_FILE
    if [ -z "$author_branches" ]; then
        echo "  (No branches detected)" >> $OUTPUT_FILE
    else
        echo "$author_branches" | sed 's/^/  - /' >> $OUTPUT_FILE
    fi

    # Lines added and removed
    stats=$(git log --all --since="$SINCE_DATE" --author="$author" --pretty=tformat: --numstat | awk '{added+=$1; removed+=$2} END {print added, removed}')
    added=$(echo $stats | awk '{print $1}')
    removed=$(echo $stats | awk '{print $2}')
    total_changed=$((added + removed))
    echo "Lines Added: $added" >> $OUTPUT_FILE
    echo "Lines Removed: $removed" >> $OUTPUT_FILE
    echo "Total Lines Changed: $total_changed" >> $OUTPUT_FILE

    # Diagrams created (checking committed image files)
    diagram_files=$(git log --all --since="$SINCE_DATE" --author="$author" --name-only --pretty=format: | grep -Ei '\.(png|jpg|jpeg|svg|drawio|pdf)$' | sort | uniq | wc -l)
    echo "Diagrams Created: $diagram_files" >> $OUTPUT_FILE

    # Non-Negligible Work
    if [[ $commit_count -ge 3 || $total_changed -ge 50 ]]; then
        echo "Non-Negligible Work: YES" >> $OUTPUT_FILE
    else
        echo "Non-Negligible Work: NO" >> $OUTPUT_FILE
    fi

    echo "" >> $OUTPUT_FILE
done

echo "✅ Report generated successfully: $OUTPUT_FILE"
