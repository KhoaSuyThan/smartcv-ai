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
  'Modern Blue Sidebar': { component: ModernBlueSidebar, defaultVisibility: ModernBlueSidebarVis },
  'Modern Brown Professional': { component: ModernBrownProfessional, defaultVisibility: ModernBrownProfessionalVis },
  'Elegant Accountant': { component: ElegantAccountant, defaultVisibility: ElegantAccountantVis },
  'Đảo Phú Quý': { component: DaoPhuQuy, defaultVisibility: DaoPhuQuyVis },
  'Mẫu CV Professional Blue - Ngô Hải Yến': { component: NgoHaiYen, defaultVisibility: NgoHaiYenVis },
  'Professional Green - Đinh Xuân Thảo': { component: DinhXuanThao, defaultVisibility: DinhXuanThaoVis },
  'Mẫu CV Pink Elegant - Nguyễn Yên Nhi': { component: NguyenYenNhi, defaultVisibility: NguyenYenNhiVis },
  'Mẫu CV Academic Brown - Nguyễn Minh An': { component: NguyenMinhAn, defaultVisibility: NguyenMinhAnVis },
  'Trần Mạnh Dũng': { component: TranManhDung, defaultVisibility: TranManhDungVis },
  'Nguyễn Võ Lê Khoa': { component: NguyenVoLeKhoa, defaultVisibility: NguyenVoLeKhoaVis },
  'Lê Chiến': { component: LeChien, defaultVisibility: LeChienVis },
  'Đặng Ngọc Linh': { component: DangNgocLinh, defaultVisibility: DangNgocLinhVis },
  'Modern Professional Split': { component: ModernProfessionalSplit, defaultVisibility: ModernProfessionalSplitVis },
  'Pastel Beige Blocks': { component: PastelBeigeBlocks, defaultVisibility: PastelBeigeBlocksVis },
  'Trần Hoài Thu': { component: TranHoaiThu, defaultVisibility: TranHoaiThuVis }
};

export default TemplateRegistry;
