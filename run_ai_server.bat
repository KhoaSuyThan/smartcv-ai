@echo off
echo Dang cai dat moi truong Python (chi mat thoi gian o lan dau tien)...
cd AiMatchService
if not exist venv (
    python -m venv venv
)
call venv\Scripts\activate
echo Dang cai dat thu vien (Neu chua co)...
pip install -r requirements.txt
echo Moi truong da san sang! Dang chay Server AI...
python main.py
pause
