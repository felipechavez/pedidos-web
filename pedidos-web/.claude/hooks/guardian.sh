#!/usr/bin/env bash
# guardian.sh · Hook PreToolUse de pedidos-web
#
# Segunda línea de defensa para las reglas de "permissions" en settings.json.
# Las reglas Read(...) y Edit(...) no ven lo que pasa dentro de un comando de
# Bash o PowerShell (por ejemplo `cat .env` o `Get-Content clave.pem`); este
# hook sí lo revisa.
#
#   - Leer .env o *.pem (Read, Grep, Bash, PowerShell)      -> deny
#   - curl, wget, Invoke-WebRequest / iwr (Bash, PowerShell) -> ask
#   - Tocar migracion/golden-master.csv (edición o shell)    -> ask
#
# Recibe el evento en JSON por stdin y responde con JSON por stdout (exit 0).
# Usa jq si está instalado; si no, revisa el JSON crudo completo.

set -u
shopt -s nocasematch   # Windows no distingue mayúsculas en rutas ni cmdlets

entrada=$(cat)

herramienta=""
objetivo=""
if command -v jq >/dev/null 2>&1; then
  herramienta=$(printf '%s' "$entrada" | jq -r '.tool_name // empty' 2>/dev/null)
  objetivo=$(printf '%s' "$entrada" | jq -r '.tool_input | (.command // .file_path // .notebook_path // .path // empty)' 2>/dev/null)
fi
# Sin jq, o si jq falla: no se deja pasar a ciegas, se revisa el JSON crudo.
if [[ -z $herramienta || -z $objetivo ]]; then
  herramienta=$(printf '%s' "$entrada" | sed -n 's/.*"tool_name"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' | head -n1)
  objetivo=$entrada
fi

# Normaliza separadores de Windows (\ y \\) a /
objetivo=${objetivo//\\//}

responder() { # $1 = deny | ask, $2 = motivo (sin comillas dobles)
  printf '{"hookSpecificOutput":{"hookEventName":"PreToolUse","permissionDecision":"%s","permissionDecisionReason":"guardian: %s"}}\n' "$1" "$2"
  exit 0
}

re_secreto='(^|[^[:alnum:]_.-])\.env($|[^[:alnum:]_.-])|\.pem($|[^[:alnum:]_])'
re_red='(^|[^[:alnum:]_-])(curl|wget|invoke-webrequest|iwr)($|[^[:alnum:]_-])'
re_golden='migracion/+golden-master\.csv'   # /+ : en JSON crudo \\ queda como //

case "$herramienta" in
  Read|Grep|Bash|PowerShell)
    if [[ $objetivo =~ $re_secreto ]]; then
      responder deny "acceso a .env o *.pem bloqueado por la politica de IA (secretos)."
    fi
    ;;
esac

case "$herramienta" in
  Bash|PowerShell)
    if [[ $objetivo =~ $re_red ]]; then
      responder ask "comando de red (curl, wget, Invoke-WebRequest). Confirma destino y datos enviados."
    fi
    if [[ $objetivo =~ $re_golden ]]; then
      responder ask "el comando toca migracion/golden-master.csv. Confirma que no lo modifica sin aprobacion."
    fi
    ;;
  Edit|Write|MultiEdit|NotebookEdit)
    if [[ $objetivo =~ $re_golden ]]; then
      responder ask "edicion de migracion/golden-master.csv requiere aprobacion de una persona."
    fi
    ;;
esac

exit 0
