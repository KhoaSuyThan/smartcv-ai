using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DoAnCS.Models
{
    // Đại diện cho một lượt chạy kiểm thử tự động
    public class TestRun
    {
        [Key]
        public int TestRunID { get; set; }

        // Thời gian thực hiện kiểm thử
        public DateTime ExecutionTime { get; set; } = DateTime.Now;

        // Tên bộ kiểm thử: 'System Health Check' hoặc 'E2E Flow Testing'
        public string SuiteName { get; set; }

        // Tổng số lượng ca kiểm thử đã chạy
        public int TotalCases { get; set; }

        // Số lượng ca kiểm thử thành công
        public int PassedCases { get; set; }

        // Số lượng ca kiểm thử thất bại
        public int FailedCases { get; set; }

        // Thời gian chạy trung bình (mili giây)
        public long AvgResponseTimeMs { get; set; }

        // Danh sách chi tiết các ca kiểm thử trong lượt này
        public List<TestCaseDetail> Details { get; set; } = new List<TestCaseDetail>();
    }
}
