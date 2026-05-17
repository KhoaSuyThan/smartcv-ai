USE DoAnWebCS;
GO

INSERT INTO [dbo].[VueTemplates] 
    ([TemplateName], [ComponentName], [ThumbnailUrl], [IsPremium], [IsActive], [Category], [CreatedAt])
VALUES 
    -- 1. Lê Minh Anh
    (N'Lê Minh Anh', N'LeMinhAnh', N'https://static.topcv.vn/cms/mau-cv-ke-toan-noi-bo-topcv69e74aa3d82da.jpg', 1, 1, N'Kế toán, Văn phòng, Chuyên nghiệp', GETDATE()),
    
    -- 2. Nguyễn Thị Lan Anh
    (N'Nguyễn Thị Lan Anh', N'NguyenThiLanAnh', N'https://cv.timviec.com.vn/images/detail/thumb_v2/mau_72.jpg?v=2', 1, 1, N'Kế toán, Marketing, Chuyên nghiệp', GETDATE()),
    
    -- 3. Trần Hoài Thu
    (N'Trần Hoài Thu', 'Template', '/images/templates/template_476e4073.png', 1, 1, N'IT, Thực tập, Chuyên nghiệp', GETDATE()),
    
    -- 4. Nguyễn Yến Nhi
    (N'Nguyễn Yến Nhi', 'NguyenYenNhi', 'https://marketplace.canva.com/EAGSZ3G6wMw/2/0/1131w/canva-s%C6%A1-y%E1%BA%BFu-l%C3%BD-l%E1%BB%8Bch-chuy%C3%AAn-nghi%E1%BB%87p-hi%E1%BB%87n-%C4%91%E1%BA%A1i-n%E1%BB%AF-t%C3%ADnh-thanh-l%E1%BB%8Bch-h%E1%BB%93ng-tr%E1%BA%AFng-N3-BrRHpD_E.jpg', 1, 1, N'Kinh doanh, Marketing, Sáng tạo', GETDATE()),
    
    -- 5. CV Nguyễn Võ Lê Khoa
    (N'CV Nguyễn Võ Lê Khoa', 'NguyenVoLeKhoa', '/images/templates/template_5119c646.png', 1, 1, N'IT, Sáng tạo', GETDATE()),
    
    -- 6. NhatNam
    (N'NhatNam', 'NhatNam', '/images/templates/144486e4-9e16-47d8-98d1-159ecfea3ced.png', 1, 1, N'IT, Thực tập', GETDATE()),
    
    -- 7. Luệ Tâm
    (N'Luệ Tâm', 'LuongLueTam', 'https://images.careerviet.vn/content/images/mau-cv-tieng-viet-careerbuilder-10.jpg?fbclid=IwY2xjawRS0-dleHRuA2FlbQIxMABicmlkETFXWUdraHlQQmFnc2d2YlpOc3J0YwZhcHBfaWQQMjIyMDM5MTc4ODIwMDg5MgABHvHP9WmiOXwIvx7qRvLDJOh0XnrmDMdANAdw8p7g2inneMSMelmC4UIx9cQ7_aem_h0K-t4TYnpfG9yidSQORyw', 1, 1, N'Kinh doanh', GETDATE()),
    
    -- 8. Vũ Tùng Dương
    (N'Vũ Tùng Dương', 'VuTungDuong', '/images/templates/114946.png', 1, 1, N'Marketing, Sáng tạo', GETDATE()),
    
    -- 9. Nguyễn Huyền Trang
    (N'Nguyễn Huyền Trang', 'NguyenHuyenTrang', '/images/templates/132438.png', 1, 1, N'Tài chính', GETDATE()),
    
    -- 10. Modern Blue Sidebar
    (N'Modern Blue Sidebar', 'ModernBlueSidebar', '/images/templates/templatesCV_1.jpg', 1, 1, N'Kinh tế, Marketing, Sáng tạo', GETDATE()),
    
    -- 11. Modern Brown Professional
    (N'Modern Brown Professional', 'ModernBrownProfessional', '/images/templates/templatesCV_2.jpg', 1, 1, N'Kinh tế, Marketing, Sáng tạo', GETDATE()),
    
    -- 12. Elegant Accountant
    (N'Elegant Accountant', 'ElegantAccountant', '/images/templates/templatesCV_3.jpg', 1, 1, N'Marketing, Sáng tạo', GETDATE()),
    
    -- 13. Đảo Phú Quý
    (N'Đảo Phú Quý', 'DaoPhuQuyCV', 'https://cdn1.vieclam24h.vn/images/assets/img/072-blue-simple-professional.jpg?v=1', 1, 1, N'IT, Marketing', GETDATE()),
    
    -- 14. Mẫu CV Academic Brown - Nguyễn Minh An
    (N'Mẫu CV Academic Brown - Nguyễn Minh An', 'AcademicBrownCV', 'https://careers.langmaster.edu.vn/storage/images/2023/05/11/mau-cv-dep-25.webp', 1, 1, N'Sáng tạo, Khác', GETDATE()),
    
    -- 15. Mẫu CV Professional Blue - Ngô Hải Yến
    (N'Mẫu CV Professional Blue - Ngô Hải Yến', 'ProfessionalBlueCV', 'https://images.careerviet.vn/content/images/tai-mau-cv-xin-viec-file-pdf-careerbuilder-5.jpg', 1, 1, N'IT, Kinh tế, Marketing', GETDATE()),
    
    -- 16. Hoàng Tường Vy
    (N'Hoàng Tường Vy', 'HoangTuongVy', '/images/templates/HoangTuongVy.png', 1, 1, N'Marketing', GETDATE()),
    
    -- 17. Pastel Beige Blocks
    (N'Pastel Beige Blocks', 'PastelBeigeBlocks', '/images/templates/PastelBeigeBlocks.png', 0, 1, N'Sáng tạo, Marketing, Hiện đại', GETDATE()),
    
    -- 18. Nguyễn Thành Nhất Nam
    (N'Nguyễn Thành Nhất Nam', N'NguyenThanhNhatNam', N'/images/templates/56b4434b_z7706213773805_d32e4083943d756e77acee513997f69a.jpg', 1, 1, N'IT, Thực tập, Thiết kế tối giản', GETDATE()),
    
    -- 19. Lê Chiến
    (N'Lê Chiến', 'LeChien', 'https://www.topcv.vn/cv/snapshot/template-cv-position/mau-cv-lap-trinh-vien-mau-thanh-lich-Xl5SXVReBF0VGg0PRFkVBwhSXFADDQIBAAEGA1MNWlYEV1NXUloACgQDUVABBQ1VUAEHGRYEAVIBWwBV4801.webp?t=1749574801', 1, 1, N'IT, Lập trình viên, Senior', GETDATE()),
    
    -- 20. Trần Đức Huy
    (N'Trần Đức Huy', 'TranDucHuy', '/images/templates/tran-duc-huy.png', 1, 1, N'IT, Lập trình viên', GETDATE()),
    
    -- 21. Đặng Ngọc Linh
    (N'Đặng Ngọc Linh', 'DangNgocLinh', '/images/templates/DangNgocLinh.webp', 1, 1, N'Nhân sự, Tư vấn, Chăm sóc khách hàng', GETDATE()),
    
    -- 22. Trần Mạnh Dũng
    (N'Trần Mạnh Dũng', 'TranManhDung', '/images/templates/template_b5b27380.webp', 1, 1, N'Marketing, Sáng tạo, Chuyên nghiệp', GETDATE()),
    
    -- 23. Đinh Xuân Thảo
    (N'Đinh Xuân Thảo', 'DinhXuanThao', '/img/cv-thumbnails/DinhXuanThao.png', 0, 1, 'Professional', GETDATE()),
    
    -- 24. Nguyen Tung Doanh
    (N'Nguyen Tung Doanh', 'NguyenTungDoanh', '/images/templates/TungDoanh.jpg', 1, 1, N'Designer, Hiện đại, 2 cột', GETDATE()),
    
    -- 25. Trương Mỹ Linh
    (N'Trương Mỹ Linh', 'TruongMyLinh', '/images/templates/TruongMyLinh.png', 1, 1, N'Nhân sự, Quản lý', GETDATE()),
    
    -- 26. Basic Blue
    (N'Basic Blue', 'BasicBlue', '/images/templates/BasicBlue.png', 1, 1, N'Kinh doanh, Marketing, Chuyên nghiệp', GETDATE()),
    
    -- 27. Vũ Hoàng Việt
    (N'Vũ Hoàng Việt', 'VuHoangViet', 'https://www.topcv.vn/cv/snapshot/template-cv/mau-cv-sinh-vien-3-_AwIHClUID1cFCQQAAgRUVAlYV1FSBAQDXFdWUQ2ff4.webp?https://www.topcv.vn/cv/snapshot/template-cv/mau-cv-sinh-vien-3-_AwIHClUID1cFCQQAAgRUVAlYV1FSBAQDXFdWUQ2ff4.webp?t=1763719062&color=3C9B68&template_name=student_3&lang=vi', 1, 1, N'Kế toán, Thực tập, Chuyên nghiệp', GETDATE()),
    
    -- 28. Nguyễn Khánh Huyền
    (N'Nguyễn Khánh Huyền', 'NguyenKhanhHuyen', '/images/templates/NguyenKhanhHuyen.png', 1, 1, N'Kế toán, Chuyên nghiệp, Văn phòng', GETDATE()),
    
    -- 29. Samira Nguyễn
    (N'Samira Nguyễn', 'SamiraNguyen', 'https://careers.langmaster.edu.vn/storage/images/2023/10/09/cv-nhan-vien-ban-hang-1.webp', 1, 1, N'Bán hàng, Kinh doanh, Chuyên nghiệp', GETDATE()),
    
    -- 30. Nguyễn Mai Anh (IT Browser)
    (N'Nguyễn Mai Anh (IT Browser)', 'NguyenMaiAnh', 'https://www.topcv.vn/v4/image/cv-template/screenshots/thumbs/cv-template-thumbnails-v1.4/vi/chrome.webp?v=3.5&lang=vi', 1, 1, N'IT, Lập trình viên, Sáng tạo, Khối bo tròn', GETDATE()),
    
    -- 31. Blue Style Professional
    (N'Blue Style Professional', 'BlueStyle', '/images/templates/BlueStyle.png', 1, 1, N'Chuyên nghiệp, 2 cột, Xanh dương', GETDATE());
