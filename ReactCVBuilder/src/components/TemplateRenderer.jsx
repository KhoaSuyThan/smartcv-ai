import React from 'react';
import DangNgocLinh from './Templates/DangNgocLinh';
import DaoPhuQuy from './Templates/DaoPhuQuy';
import DinhXuanThao from './Templates/DinhXuanThao';
import ElegantAccountant from './Templates/ElegantAccountant';
import LeChien from './Templates/LeChien';
import ModernBlueSidebar from './Templates/ModernBlueSidebar';
import ModernBrownProfessional from './Templates/ModernBrownProfessional';
import ModernProfessionalSplit from './Templates/ModernProfessionalSplit';
import NgoHaiYen from './Templates/NgoHaiYen';
import NguyenMinhAn from './Templates/NguyenMinhAn';
import NguyenVoLeKhoa from './Templates/NguyenVoLeKhoa';
import NguyenYenNhi from './Templates/NguyenYenNhi';
import PastelBeigeBlocks from './Templates/PastelBeigeBlocks';
import TranHoaiThu from './Templates/TranHoaiThu';
import TranManhDung from './Templates/TranManhDung';

const components = {
    DangNgocLinh, DaoPhuQuy, DinhXuanThao, ElegantAccountant, LeChien,
    ModernBlueSidebar, ModernBrownProfessional, ModernProfessionalSplit,
    NgoHaiYen, NguyenMinhAn, NguyenVoLeKhoa, NguyenYenNhi,
    PastelBeigeBlocks, TranHoaiThu, TranManhDung
};

const TemplateRenderer = ({ templateName, resumeData }) => {
    const ComponentToRender = components[templateName];
    
    if (!ComponentToRender) {
        return <div>Template {templateName} not found!</div>;
    }

    // Wrap in A4 container size for uniform preview
    return (
        <div style={{ width: '210mm', minHeight: '297mm', background: 'white' }}>
            <ComponentToRender resumeData={resumeData} />
        </div>
    );
};

export default TemplateRenderer;
