#!/bin/sh
# GitHub etiketlerini ve milestone'larını oluşturur (P0-22).
# Gereksinim: GitHub CLI (gh) kurulu ve `gh auth login` yapılmış olmalı.
# Kullanım: repo klasöründe `sh scripts/setup-github.sh`
set -e

label() { gh label create "$1" --color "$2" --description "$3" --force; }

# Tür
label "type:feature" "1d76db" "Yeni özellik"
label "type:bug"     "d73a4a" "Hata"
label "type:docs"    "0075ca" "Dokümantasyon"
label "type:chore"   "cfd3d7" "Bakım, altyapı"
label "type:adr"     "5319e7" "Mimari karar"

# Öncelik (MoSCoW)
label "priority:must"   "b60205" "Must"
label "priority:should" "fbca04" "Should"
label "priority:could"  "c2e0c6" "Could"

# Modüller
for m in tenancy identity organization groups students enrollments attendance billing announcements; do
  label "module:$m" "bfdadc" "$m modülü"
done
label "area:admin-web" "bfdadc" "Angular admin uygulaması"
label "area:infra"     "bfdadc" "CI/CD, Docker, sunucu"

# Milestone'lar
REPO=$(gh repo view --json nameWithOwner -q .nameWithOwner)
milestone() { gh api "repos/$REPO/milestones" -f title="$1" -f description="$2" >/dev/null && echo "Milestone: $1"; }
milestone "Phase 0 – Foundation (v0.0.1)" "Planlama, repo, walking skeleton"
milestone "Phase 1 – MVP (v0.1.0)"        "MVP: okullar, gruplar, öğrenciler, kayıt, yoklama"

echo "Tamamlandı."
