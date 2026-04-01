#!/bin/bash
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
OUT="$SCRIPT_DIR/unity_structure.txt"
ASSETS="${1:-.}"

echo "Unity Project Structure" > "$OUT"
echo "Generated: $(date)" >> "$OUT"
echo "Root: $(pwd)" >> "$OUT"
echo "" >> "$OUT"

TMPFILE=$(mktemp)
find "$ASSETS" -name "*.cs" | sort > "$TMPFILE"

while IFS= read -r file; do
    namespace=$(grep -m1 -E '^\s*namespace\s+' "$file" | sed 's/.*namespace[[:space:]]\{1,\}//' | sed 's/[[:space:]].*//')

    while IFS= read -r decl_line; do
        lineno=$(echo "$decl_line" | cut -d: -f1)
        content=$(echo "$decl_line" | cut -d: -f2-)

        kind=$(echo "$content" | grep -oE '\b(class|struct|interface|enum)\b' | head -1)
        name=$(echo "$content" | grep -oE "\b(class|struct|interface|enum)\s+[A-Za-z0-9_]+" | sed "s/.*\b\(class\|struct\|interface\|enum\)[[:space:]]\{1,\}//")
        parents=$(echo "$content" | sed -n 's/.*:[[:space:]]*\([A-Za-z0-9_<>, ]*\)/\1/p' | head -1)
        modifiers=$(echo "$content" | grep -oE '^\s*(public|internal|protected|private)' | tr -d ' ')
        extras=$(echo "$content" | grep -oE '\b(static|abstract|sealed|partial)\b' | tr '\n' ' ' | sed 's/[[:space:]]*$//')

        echo "" >> "$OUT"
        echo "━━━ $file" >> "$OUT"
        ns_part="${namespace:+ (${namespace})}"
        echo "  $kind $name$ns_part" >> "$OUT"
        [[ -n "$parents" ]] && echo "  : $parents" >> "$OUT"
        mod_str="${modifiers}${extras:+ $extras}"
        [[ -n "$mod_str" ]] && echo "  modifiers: $mod_str" >> "$OUT"

        # From the declaration line onward, extract members using grep
        tail -n "+$lineno" "$file" | grep -E '^\s*(public|protected|internal)\s+' | while IFS= read -r line; do
            # Skip re-declarations
            echo "$line" | grep -qE '\b(class|struct|interface|enum)\b' && continue

            if echo "$line" | grep -qE '\('; then
                # Method
                sig=$(echo "$line" | sed 's/^[[:space:]]*//' | grep -oE '(public|protected|internal)[^(]+\([^)]*\)' | head -1)
                [[ -n "$sig" ]] && echo "    method      $sig" >> "$OUT"
            elif echo "$line" | grep -qE '[;={]'; then
                # Field or property
                sig=$(echo "$line" | sed 's/^[[:space:]]*//' | grep -oE '(public|protected|internal)\s+(static\s+)?(readonly\s+)?[A-Za-z0-9_<>\[\]]+\s+[A-Za-z0-9_]+' | head -1)
                [[ -n "$sig" ]] && echo "    field/prop  $sig" >> "$OUT"
            fi
        done

        # SerializeField — grep pairs directly
        grep -n '\[SerializeField\]' "$file" | while IFS= read -r sf_line; do
            sf_lineno=$(echo "$sf_line" | cut -d: -f1)
            next_lineno=$((sf_lineno + 1))
            field=$(sed -n "${next_lineno}p" "$file" | grep -oE '[A-Za-z0-9_<>\[\]]+[[:space:]]+[A-Za-z0-9_]+[[:space:]]*;' | head -1)
            [[ -n "$field" ]] && echo "    serialized  $field" >> "$OUT"
        done

    done < <(grep -n '\b\(class\|struct\|interface\|enum\)\b' "$file")

done < "$TMPFILE"
rm -f "$TMPFILE"

echo "" >> "$OUT"
COUNT=$(grep -c '━━━' "$OUT")
echo "Done. $COUNT classes processed." >> "$OUT"
echo "Output written to: $OUT"
echo "$COUNT classes found."
