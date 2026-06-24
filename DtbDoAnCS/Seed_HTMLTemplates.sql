USE DoAnWebCS;
GO

-- 1. Xóa mẫu cũ nếu tồn tại
DELETE FROM Templates WHERE Name = N'Modern Blue Sidebar';
GO

-- thêm template 15 cv react 
INSERT INTO [DoAnWebCS].[dbo].[Templates] 
    ([Name], [HtmlContent], [CssContent], [PreviewImageUrl], [IsActive], [IsProOnly], [Category])
VALUES
(N'Modern Blue Sidebar', NULL, NULL, N'/images/templates/templatesCV_1.jpg', 1, 1, N'Kinh tế, Marketing, Sáng tạo'),
(N'Modern Brown Professional', NULL, NULL, N'/images/templates/templatesCV_2.jpg', 1, 1, N'IT, Kinh tế'),
(N'Elegant Accountant', NULL, NULL, N'/images/templates/templatesCV_3.jpg', 1, 0, N'Marketing, Sáng tạo'),
(N'Đảo Phú Quý', NULL, NULL, N'https://cdn1.vieclam24h.vn/images/assets/img/072-blue-simple-professional.jpg?v=1', 1, 0, N'IT, Marketing'),
(N'Mẫu CV Professional Blue - Ngô Hải Yến', NULL, NULL, N'https://images.careerviet.vn/content/images/tai-mau-cv-xin-viec-file-pdf-careerbuilder-5.jpg', 1, 0, N'IT, Kinh tế, Marketing'),
(N'Professional Green - Đinh Xuân Thảo', NULL, NULL, N'https://static.vietcv.io/image/vng/confidential/c4c87802f4cadd024416c22a12314e88.png?_=1645622482', 1, 0, N'Kinh tế, Marketing'),
(N'Mẫu CV Pink Elegant - Nguyễn Yên Nhi', NULL, NULL, N'https://marketplace.canva.com/EAGSZ3G6wMw/2/0/1131w/canva-s%C6%A1-y%E1%BA%BFu-l%C3%BD-l%E1%BB%8Bch-chuy%C3%AAn-nghi%E1%BB%87p-hi%E1%BB%87n-%C4%91%E1%BA%A1i-n%E1%BB%AF-t%C3%ADnh-thanh-l%E1%BB%8Bch-h%E1%BB%93ng-tr%E1%BA%AFng-N3-BrRHpD_E.jpg', 1, 1, N'Marketing, Sáng tạo'),
(N'Mẫu CV Academic Brown - Nguyễn Minh An', NULL, NULL, N'https://careers.langmaster.edu.vn/storage/images/2023/05/11/mau-cv-dep-25.webp', 1, 0, N'Sáng tạo, Khác'),
(N'Trần Mạnh Dũng', NULL, NULL, N'/images/templates/template_b5b27380.webp', 1, 1, N'IT, Kinh tế'),
(N'Nguyễn Võ Lê Khoa', NULL, NULL, N'/images/templates/template_5119c646.png', 1, 1, N'IT, Sáng tạo'),
(N'Lê Chiến', NULL, NULL, N'https://www.topcv.vn/cv/snapshot/template-cv-position/mau-cv-lap-trinh-vien-mau-thanh-lich-Xl5SXVReBF0VGg0PRFkVBwhSXFADDQIBAAEGA1MNWlYEV1NXUloACgQDUVABBQ1VUAEHGRYEAVIBWwBV4801.webp?t=1749574801', 1, 0, N'IT'),
(N'Đặng Ngọc Linh', NULL, NULL, N'https://www.topcv.vn/cv/snapshot/template-cv/mau-cv-senior-_B1tfBFEAUgALBAgBBl0MD1wECQBQAQFQAwIAAw44ec.webp?t=1756265781&color=000000&template_name=senior_v2&lang=vi', 1, 1, N'IT, Kinh tế'),
(N'Modern Professional Split', NULL, NULL, N'/images/templates/e65c2855_z7705180115926_56b6cdf58b811d5b001a6ce24c2c1ce8.jpg', 1, 0, N'IT, Marketing'),
(N'Pastel Beige Blocks', NULL, NULL, N'/images/templates/56b4434b_z7706213773805_d32e4083943d756e77acee513997f69a.jpg', 1, 0, N'IT, Sáng tạo'),
(N'Trần Hoài Thu', NULL, NULL, N'/images/templates/template_476e4073.png', 1, 0, N'Kinh tế, Marketing, Sáng tạo');
