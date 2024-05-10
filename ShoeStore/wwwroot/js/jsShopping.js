$(document).ready(function () {
    ShowCount();
    var sizea;
    $('.product_size li').click(function () {
        sizea = $(this).text();
    });
    $('body').on('click', '.btnAddToCart', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var quantity = 1;
        var size = sizea;
        var tQuantity = $('#quantity_value').text();
        if (tQuantity != '') {
            quantity = parseInt(tQuantity);
        }
        if (!size) { // Kiểm tra nếu size chưa được chọn
            alert('Vui lòng chọn size');
            return;
        }
        $.ajax({
            url: '/shoppingcart/addtocart',
            type: 'POST',
            data: { id: id, size: size, quantity: quantity },
            success: function (rs) {
                if (rs.success) {
                    $('#checkout_items').html(rs.count); 
                    alert(rs.msg); 
                }
            }
        });
    });
    $('body').on('click', '.btnUpdate', function (e) {
        e.preventDefault();
        var id = $(this).data("id");
        var quantity = $('#Quantity_' + id).val();
        Update(id, quantity);

    });
    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var conf = confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?');
        if (conf == true) {
            $.ajax({
                url: '/shoppingcart/delete',
                type: 'POST',
                data: { id: id },
                success: function (rs) {
                    if (rs.success) {
                        $('#checkout_items').html(rs.count);
                        $('#trow_' + id).remove();
                        alert(rs.msg); 
                    }
                }
            });
        }

    });
});
function ShowCount() {
    $.ajax({
        url: '/shoppingcart/showcount',
        type: 'GET',
        success: function (rs) {
            if (rs.success) {
                alert('123')
                $('#checkout_items').html(rs.count);
            }
        }
    });
}
function DeleteAll() {
    $.ajax({
        url: '/shoppingcart/DeleteAll',
        type: 'POST',
        success: function (rs) {
            if (rs.Success) {
                alert(rs.msg);
            }
        }
    });
}
function Update(id, quantity) {
    $.ajax({
        url: '/shoppingcart/Update',
        type: 'POST',
        data: { id: id, quantity: quantity },
        success: function (rs) {
            if (rs.Success) {
                LoadCart();
            }
        }
    });
}
(function () {
    function initial() {
        registerEvents();
    }
    function registerEvents() {
        $(document).on('blur', '.txt-quantity-cart', function () {
            var self = $(this);
            var parentTr = self.closest('tr');
            var price = parseFloat(parentTr.find('.txt-price').text().replaceAll('.', ''));
            var quantity = parseFloat(self.val());
            var total = price * quantity;
            parentTr.find('.txt-total').text(total.toLocaleString("vi-Vn", {
                style: 'currency',
                currency: 'VND'
            }));
            var totalCart = 0;
            var trs = $('#tbody-cart tr');
            for (var i = 0; i < trs.length; i++) {
                if (i === trs.length - 1) {
                    break;
                }
                var total = parseFloat($(trs[i]).find('.txt-total').text().replaceAll('đ', '').replaceAll('.', ''));
                totalCart += total;
            }
            $('#txt-total-cart').text(totalCart.toLocaleString("vi-VN"), {
                style: 'currency',
                currency: 'VND'
            });
        });
    }
    initial();
});
