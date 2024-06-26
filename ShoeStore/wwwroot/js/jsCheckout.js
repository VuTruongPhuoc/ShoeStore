﻿// sự kiện checkbox để kiểm tra thông tin khách hàng muốn chọn
$(document).ready(function () {
    $("#LikeCustomers").change(function () {
        var isChecked = $(this).prop("checked");
        if (isChecked) {
            $(this).val(true)
            $('#myForm').attr('onsubmit', ''); // Loại bỏ thuộc tính onsubmit để cho phép submit form mặc định
            $(".delivery-info").addClass("d-none");
        } else {
            $(this).val(false);
            $('#myForm').attr('onsubmit', 'return validateForm()'); // Thêm thuộc tính onsubmit để chạy hàm validateForm()
            $(".delivery-info").removeClass("d-none");
        }
    });
})
document.addEventListener("DOMContentLoaded", function () {
    // hàm update tổng tiền
    function updateTotal() {
        var checkedOption = document.querySelector('input[name="selectordelivery"]:checked');
        if (checkedOption) {
            var shippingFee = parseFloat(checkedOption.value);
            var discountValue = parseFloat(document.getElementById("discountValue").value) || 0;
            var orderTotal = parseFloat(document.getElementById("tongtien").value);

            var totalAmount = orderTotal - discountValue + shippingFee;

            document.getElementById("shippingFee").textContent = shippingFee.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });
            document.getElementById("totalAmount").textContent = totalAmount.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });
            document.getElementById("shippingFee").value = shippingFee;
            document.getElementById("totalAmount").value = totalAmount;
            document.querySelector('input[name="ShipFee"]').value = shippingFee;
            document.querySelector('input[name="totalAmount"]').value = totalAmount;
        }
    }
    // update lại tổng tiền khi tải trang
    updateTotal();
    // update tổng tiền khi thay đổi phương thưucs vận chuyển
    document.querySelectorAll('input[name="selectordelivery"]').forEach((e) => {
        e.addEventListener("change", updateTotal);
    });
    // sự kiện ghi trên input
    document.getElementById('searchCoupon').addEventListener('input', function () {
        var searchInput = document.getElementById('searchCoupon');
        var inputValue = searchInput.value;
        if (this.value.trim() !== '') {
            applyButton.removeAttribute('style');

        } else {
            applyButton.style.pointerEvents = 'none';
            applyButton.style.opacity = '0.5';
        }
    });
    //sự kiện click để chạy hàm update voucher
    $(document).on('click', '#applyButton', function () {
        var inputValue = $('#searchCoupon').val(); // Sử dụng jQuery để lấy giá trị của trường văn bản
        applyvoucher(inputValue);
    });
    // update giảm giá khi sử dụng voucher từ kho voucher của bạn
    var selectedVoucher = selectedVoucherck;
    if (selectedVoucher !== '') {
        applyvoucher(selectedVoucher);
    }
    // update giảm giá khi chọn ô sử dụng
    $(document).on('click', '.apply-btn', function () {
        var selectedVoucher = $(this).data('code');
        applyvoucher(selectedVoucher)
    });
    //hàn update voucher
    function applyvoucher(selectedVoucher) {
        $.ajax({
            type: 'POST',
            /*url: '@Url.Action("ApplyDiscount", "ShoppingCart")',*/
            url: '/shoppingcart/applydiscount',
            data: { selectedVoucher: selectedVoucher },
            success: function (rs) {
                if (rs.success) {
                    document.getElementById("discountValue").textContent = rs.discountValue.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });
                    document.getElementById("discountValue").value = rs.discountValue;
                    $('#showname').html("Mã giảm : ");
                    $('#showcodevoucher').html(selectedVoucher);
                    document.querySelector('input[name="VoucherCode"]').value = selectedVoucher;
                    document.querySelector('input[name="discountValue"]').value = rs.discountValue;
                    $('#couponModal').modal('hide');
                } else {
                    alert(rs.message);
                }

                updateTotal();
            }
        });
    }
});
// phương thức thanh toán
var inputon = document.getElementById("f-option6");
var inputoff = document.getElementById("f-option5");
inputon.onclick = function () {
    document.getElementById("pay").value = "Online"
}
inputoff.onclick = function () {
    document.getElementById("pay").value = "shipCod"
}
// validate phonenumber and email

//validate các trường
function validateForm() {
    var fullName = document.getElementById('customername').value;
    var phoneNumber = document.getElementById('phone').value;
    var Address = document.getElementById('address').value;
    var Email = document.getElementById('email').value;

    // var address = document.querySelector('input[name="Address"]').value;
    var city = document.getElementById('city');
    var district = document.getElementById('district');
    var ward = document.getElementById('ward');

    var cityName = city.options[city.selectedIndex].text;
    var districtName = district.options[district.selectedIndex].text;
    var wardName = ward.options[ward.selectedIndex].text;
    document.querySelector('input[name="City"]').value = cityName;
    document.querySelector('input[name="District"]').value = districtName;
    document.querySelector('input[name="Ward"]').value = wardName;

    var city = document.querySelector('[name="City"]').value;
    var district = document.querySelector('[name="District"]').value;
    var ward = document.querySelector('[name="Ward"]').value;
    // Kiểm tra xem các trường có rỗng hay không
    if (fullName.trim() === '' || phoneNumber.trim() === '' || Email.trim() === '' || Address.trim() === '' || city.trim() === 'Chọn tỉnh thành' || district.trim() === 'Chọn quận huyện' || ward.trim() === 'Chọn phường xã') {
        Swal.fire({
            title: "Hãy điền đầy đủ thông tin",
            icon: "warning",
            confirmButtonText: "Ok",
            position: 'center',
        })
        return false; // Ngăn chặn form submit
    }

    // ...
    return true; // Cho phép form submit
}