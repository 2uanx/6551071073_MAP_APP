using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.TienNghi
{
    public partial class FormThemTienNghi : Form
    {
        public FormThemTienNghi()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AcceptButton = btnThemMoi;
            CancelButton = btnThoat;

            txtMaTN.MaxLength = 5;
            txtTenTN.MaxLength = 50;
        }

        private void btnThemMoi_Click(object sender, EventArgs e)
        {
            string maTN = txtMaTN.Text.Trim().ToUpper();
            string tenTN = txtTenTN.Text.Trim();

            if (string.IsNullOrEmpty(maTN))
            {
                MessageBox.Show("Vui lòng nhập mã tiện nghi!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMaTN.Focus();
                return;
            }

            if (string.IsNullOrEmpty(tenTN))
            {
                MessageBox.Show("Vui lòng nhập tên tiện nghi!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenTN.Focus();
                return;
            }

            try
            {
                using var db = new HotelManagementContext();

                var tnCu = db.TienNghis.FirstOrDefault(t => t.MaTn == maTN);
                if (tnCu != null)
                {
                    if (tnCu.DaXoa != true)
                    {
                        MessageBox.Show("Mã tiện nghi đã tồn tại!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtMaTN.Focus();
                        return;
                    }

                    var traLoi = MessageBox.Show(
                        $"Mã tiện nghi {maTN} từng bị xóa trước đây. Bạn có muốn khôi phục lại không?",
                        "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (traLoi != DialogResult.Yes)
                    {
                        txtMaTN.Focus();
                        return;
                    }

                    tnCu.TenTn = tenTN;
                    tnCu.DaXoa = false;
                }
                else
                {
                    if (db.TienNghis.Any(t => t.TenTn == tenTN && t.DaXoa != true))
                    {
                        MessageBox.Show("Tên tiện nghi đã tồn tại!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtTenTN.Focus();
                        return;
                    }

                    db.TienNghis.Add(new Models.TienNghi
                    {
                        MaTn = maTN,
                        TenTn = tenTN,
                        DaXoa = false
                    });
                }

                int kq = db.SaveChanges();
                if (kq > 0)
                {
                    MessageBox.Show("Thêm tiện nghi thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Thêm tiện nghi thất bại!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm tiện nghi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
