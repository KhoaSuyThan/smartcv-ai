import DangNgocLinh, { defaultVisibility as DangNgocLinhVis } from './Templates/DangNgocLinh'
import DaoPhuQuy, { defaultVisibility as DaoPhuQuyVis } from './Templates/DaoPhuQuy'
import DinhXuanThao, { defaultVisibility as DinhXuanThaoVis } from './Templates/DinhXuanThao'
import ElegantAccountant, { defaultVisibility as ElegantAccountantVis } from './Templates/ElegantAccountant'
import LeChien, { defaultVisibility as LeChienVis } from './Templates/LeChien'
import ModernBlueSidebar, { defaultVisibility as ModernBlueSidebarVis } from './Templates/ModernBlueSidebar'
import ModernBrownProfessional, { defaultVisibility as ModernBrownProfessionalVis } from './Templates/ModernBrownProfessional'
import ModernProfessionalSplit, { defaultVisibility as ModernProfessionalSplitVis } from './Templates/ModernProfessionalSplit'
import NgoHaiYen, { defaultVisibility as NgoHaiYenVis } from './Templates/NgoHaiYen'
import NguyenMinhAn, { defaultVisibility as NguyenMinhAnVis } from './Templates/NguyenMinhAn'
import NguyenVoLeKhoa, { defaultVisibility as NguyenVoLeKhoaVis } from './Templates/NguyenVoLeKhoa'
import NguyenYenNhi, { defaultVisibility as NguyenYenNhiVis } from './Templates/NguyenYenNhi'
import PastelBeigeBlocks, { defaultVisibility as PastelBeigeBlocksVis } from './Templates/PastelBeigeBlocks'
import TranHoaiThu, { defaultVisibility as TranHoaiThuVis } from './Templates/TranHoaiThu'
import TranManhDung, { defaultVisibility as TranManhDungVis } from './Templates/TranManhDung'

const TemplateRegistry = {
  'Modern Blue Sidebar': { id: 1, isProOnly: true, component: ModernBlueSidebar, defaultVisibility: ModernBlueSidebarVis },
  'Modern Brown Professional': { id: 2, isProOnly: true, component: ModernBrownProfessional, defaultVisibility: ModernBrownProfessionalVis },
  'Elegant Accountant': { id: 3, isProOnly: false, component: ElegantAccountant, defaultVisibility: ElegantAccountantVis },
  'Đảo Phú Quý': { id: 4, isProOnly: false, component: DaoPhuQuy, defaultVisibility: DaoPhuQuyVis },
  'Mẫu CV Professional Blue - Ngô Hải Yến': { id: 5, isProOnly: false, component: NgoHaiYen, defaultVisibility: NgoHaiYenVis },
  'Professional Green - Đinh Xuân Thảo': { id: 6, isProOnly: false, component: DinhXuanThao, defaultVisibility: DinhXuanThaoVis },
  'Mẫu CV Pink Elegant - Nguyễn Yên Nhi': { id: 9, isProOnly: true, component: NguyenYenNhi, defaultVisibility: NguyenYenNhiVis },
  'Mẫu CV Academic Brown - Nguyễn Minh An': { id: 10, isProOnly: false, component: NguyenMinhAn, defaultVisibility: NguyenMinhAnVis },
  'Trần Mạnh Dũng': { id: 11, isProOnly: true, component: TranManhDung, defaultVisibility: TranManhDungVis },
  'Nguyễn Võ Lê Khoa': { id: 12, isProOnly: true, component: NguyenVoLeKhoa, defaultVisibility: NguyenVoLeKhoaVis },
  'Lê Chiến': { id: 13, isProOnly: false, component: LeChien, defaultVisibility: LeChienVis },
  'Đặng Ngọc Linh': { id: 14, isProOnly: true, component: DangNgocLinh, defaultVisibility: DangNgocLinhVis },
  'Modern Professional Split': { id: 17, isProOnly: false, component: ModernProfessionalSplit, defaultVisibility: ModernProfessionalSplitVis },
  'Pastel Beige Blocks': { id: 18, isProOnly: false, component: PastelBeigeBlocks, defaultVisibility: PastelBeigeBlocksVis },
  'Trần Hoài Thu': { id: 19, isProOnly: false, component: TranHoaiThu, defaultVisibility: TranHoaiThuVis }
};

export default TemplateRegistry;
