USE DoAnWebCS;
GO

-- Nạp Kỹ năng IT
INSERT INTO Skills (SkillName) VALUES ('.NET'), ('SQL Server'), ('C#'), ('Flutter'), ('React');
GO

INSERT INTO Companies (Name, LogoUrl, Website, Address, Industry) VALUES 
(N'Baxter and Woodman Inc', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/gnezDA5qkvM_3V23SGiDNMhfOjjqA_Wj4jT-kPPNvlM.jpeg', 'https://www.indeed.com/viewjob?jk=3e227540a546a37c', N'Chicago, IL', N'IT Consulting'),
(N'Rsm Us Llp.', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/vLqbAgGTGajIdOCOXiWJPSgcVZX5T_Ohu_uq0wdzFUo.png', 'https://www.whatjobs.com/jobs/senior-project-manager-it/chicago-illinois?id=2570160112', N'Chicago, IL', N'Agile Delivery'),
(N'Contemporary Staffing', NULL, 'https://careers.contemporarystaffing.com/jobs/37462', N'Elk Grove Village, IL', N'IT Support'),
(N'McDonald''s Corporation', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/cM5DL5-_2HfX1X3jU9h2bwTwKIyXurvcAiKUJViuU3M.jpeg', 'https://www.jobzmall.com/mcdonald-s-corporation/job/analyst-penetration-testing', N'Chicago, IL', N'Cybersecurity'),
(N'Kirkland & Ellis', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/P423na0BD2YiXEZGVgmm7l-9rwDrdMdraMSCYUtESJ0.png', 'https://us.jobrapido.com/jobpreview/5072755767732338688', N'Chicago, IL', N'FinOps'),
(N'Lenovo', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/TkUNTkzdVjwXaTmgCkjX1Gad8pgzgHWHajnZnYymu7k.jpeg', 'https://www.indeed.com/viewjob?jk=9d95ff691e418663', N'Chicago, IL', N'Artificial Intelligence'),
(N'U.S. Navy', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/RRLfWjenk0dMUZ4AKgPAWGeNn2kbcmLkmwHVRJ0DySI.png', 'https://www.adzuna.com/details/5655660238', N'South Holland, IL', N'Information Warfare'),
(N'Motorola', NULL, 'https://www.ziprecruiter.com/c/Motorola-Solutions/Job/IT-Directory-Infrastructure-Lead-(Chicago-Schaumburg-Hybrid)/-in-Schaumburg,IL?jid=bc508c769e0fb0d8', N'Schaumburg, IL', N'Infrastructure'),
(N'Nexzentek Solutions', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/MC0CnGAbtG01fQQrkYgeznkEPWV7a-5IvkFbQdGIyTw.png', 'https://www.optnation.com/aws-developer-job-in-chicago-il-view-jobid-25038', N'Chicago, IL', N'AWS Cloud'),
(N'United Airlines', NULL, 'https://careers.united.com/us/en/job/WHQ00025769/Analyst-Identity-Access-Management', N'Chicago, IL', N'Aviation DT');

INSERT INTO Jobs (Title, Description, Requirements, Salary, Deadline, CompanyID, RecruiterID, Status) VALUES 
(N'IT Consultant', N'Thiết kế, hỗ trợ và bảo trì các giải pháp CNTT cho khách hàng đô thị.', N'8+ năm kinh nghiệm, thạo Windows Server, Active Directory, ảo hóa.', N'90K - 120K / year', '2026-12-31', 1, 1, 1),
(N'Senior Project Manager: IT & Agile Delivery', N'Dẫn dắt các dự án phức tạp ngân sách > $200k.', N'8-10 năm PM, 3-5 năm Agile. Ưu tiên chứng chỉ PMI.', N'Thỏa thuận', '2026-11-20', 2, 1, 1),
(N'IT Support Specialist Tier 2', N'Hỗ trợ kỹ thuật nâng cao onsite và remote cho sản xuất linh kiện ô tô.', N'Troubleshoot Windows/Mac, thạo Google Workspace/O365.', N'Thỏa thuận', '2026-10-15', 3, 1, 1),
(N'Analyst, Penetration Testing', N'Kiểm tra xâm nhập hệ thống và mạng toàn cầu để tìm lỗ hổng bảo mật.', N'Nền tảng Pentest mạnh, kỹ năng phân tích độc lập tốt.', N'98K - 120K / year', '2026-12-05', 4, 1, 1),
(N'FinOps IT Finance Analyst', N'Đảm bảo minh bạch tài chính cho các sáng kiến công nghệ và Cloud spend.', N'4-6 năm kinh nghiệm, cử nhân tài chính, thạo Excel/Analytical.', N'Cạnh tranh', '2026-09-12', 5, 1, 1),
(N'AI QA - Engineer', N'Đảm bảo hệ thống AI (LLM, Vision) chính xác và an toàn quy mô lớn.', N'3+ năm QA AI/ML, thạo Python, ML model evaluation.', N'110K - 150K / year', '2026-08-30', 6, 1, 1),
(N'Information Systems Technician', N'Vận hành và bảo vệ mạng lưới hạm đội Navy toàn cầu.', N'Cần quốc tịch Mỹ, vượt qua bài kiểm tra năng khiếu quân đội (ASVAB).', N'Theo quy định', '2026-12-31', 7, 1, 1),
(N'IT Directory Infrastructure Lead', N'Quản trị toàn cầu môi trường Active Directory và email backbone.', N'8-10 năm kinh nghiệm AD/DNS, 3+ năm vị trí lãnh đạo.', N'145K - 170K / year', '2026-07-25', 8, 1, 1),
(N'AWS Developer', N'Phát triển Cloud cho dự án dài hạn của United Airlines.', N'8+ năm kinh nghiệm, nền tảng .NET mạnh, giao tiếp tốt.', N'Theo hợp đồng', '2026-06-10', 9, 1, 1),
(N'Analyst - Identity & Access Management', N'Bảo mật hệ thống IAM, quản lý xác thực hiện đại (OIDC, SAML, SSO).', N'3+ năm kinh nghiệm, thạo Okta/Entra/SailPoint.', N'87K - 114K / year', '2026-05-18', 10, 1, 1);

-- Thêm 5 Công ty tiếp theo (ID từ 11 đến 15)
INSERT INTO Companies (Name, LogoUrl, Website, Address, Industry) VALUES 
(N'Google (Chicago Tower)', 'https://www.google.com/images/branding/googlelogo/2x/googlelogo_color_92x30dp.png', 'https://www.google.com/about/careers/applications/', N'Chicago, IL', N'Cloud & Search'),
(N'Zebra Technologies', 'https://www.zebra.com/content/dam/zebra_new_ia/en-us/solutions-verticals/product-logos/Zebra_Logo_Tagline_Black.png', 'https://www.zebra.com/us/en/about-zebra/careers.html', N'Lincolnshire, IL', N'Enterprise Technology'),
(N'Salesforce', 'https://a.sfdcstatic.com/shared/images/c360-nav/salesforce-with-type-logo.svg', 'https://careers.salesforce.com/en/jobs/', N'Chicago, IL', N'CRM & SaaS'),
(N'Grubhub', 'https://pwa-cdn.grubhub.com/beta/grubhub-assets/images/grubhub-logo-red.svg', 'https://careers.grubhub.com/', N'Chicago, IL', N'FoodTech'),
(N'Allstate', 'https://www.allstate.com/content/dam/allstate/allstate-logo-header.png', 'https://www.allstate.jobs/', N'Northbrook, IL', N'InsurTech');

-- Thêm 5 Công việc tương ứng (Liên kết với CompanyID 11-15)
INSERT INTO Jobs (Title, Description, Requirements, Salary, Deadline, CompanyID, RecruiterID, Status) VALUES 
(N'Senior Software Engineer - Site Reliability', N'Đảm bảo hệ thống hạ tầng Cloud của Google hoạt động ổn định và có khả năng mở rộng cao.', N'5+ năm kinh nghiệm với C++, Java hoặc Go. Thạo hệ thống Linux và hạ tầng mạng Cloud.', N'145K - 195K / year', '2026-12-25', 11, 1, 1),
(N'Embedded Software Engineer', N'Phát triển phần mềm nhúng cho các thiết bị quét mã vạch và máy tính di động công nghiệp.', N'Thạo C/C++, kiến trúc RTOS và vi điều khiển ARM. Hiểu biết về giao tiếp WiFi/Bluetooth.', N'95K - 135K / year', '2026-11-15', 12, 1, 1),
(N'Technical Solutions Architect', N'Thiết kế giải pháp tích hợp hệ thống CRM Salesforce cho các doanh nghiệp tài chính lớn.', N'Chứng chỉ Salesforce Architect, 8+ năm kinh nghiệm phần mềm, thạo Apex và LWC.', N'160K - 210K / year', '2026-10-30', 13, 1, 1),
(N'Senior Data Scientist', N'Phát triển các mô hình học máy để tối ưu hóa thời gian giao hàng và trải nghiệm thực khách.', N'Thạc sĩ/Tiến sĩ toán tin, 4+ năm kinh nghiệm Python/R, thạo SQL và các thư viện ML.', N'125K - 170K / year', '2026-09-20', 14, 1, 1),
(N'Cloud Security Engineer', N'Bảo mật hạ tầng Azure cho hệ thống bảo hiểm toàn cầu, quản lý Identity và mã hóa dữ liệu.', N'3+ năm bảo mật Cloud, chứng chỉ AZ-500 hoặc tương đương. Thạo Terraform/Ansible.', N'115K - 155K / year', '2026-08-05', 15, 1, 1);