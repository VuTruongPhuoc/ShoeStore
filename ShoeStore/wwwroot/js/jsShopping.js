var idproductdetail;
$(document).ready(function () {
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
                        if (rs.count == 0) {
                            location.reload();
                        }
                        UpdateTotalAndCart(id, 0);
                        
                    }
                }
            });
        }

    });
    $(".txt-quantity-cart").on("change", function () {
        idproductdetail = $(this).data("code");
        var productid = $(this).data("code"); // Lấy mã sản phẩm
        var quantity = $(this).val(); // Lấy số lượng mới
        if (quantity == 0) {

        }    
        UpdateCheckQuantity(productid, quantity);
        UpdateTotalAndCart(productid, quantity);
    });
});
function UpdateTotalAndCart(productid, quantity) {
    $.ajax({
        url: "/ShoppingCart/UpdateQuantity",
        method: "POST",
        data: { productid: productid, quantity: quantity },
        success: function (rs) {
            if (rs.success) {
                var totalPriceFormatted = rs.totalprice.toLocaleString('vi-VN', {
                    style: 'currency',
                    currency: 'VND'
                });
                $("#txt-total-cart").text(totalPriceFormatted);
                    
                var totalFormatted = rs.total.toLocaleString('vi-VN', {
                    style: 'currency',
                    currency: 'VND'
                });
                $(".txt-total[data-productid='" + productid + "']").text(totalFormatted);
               
                $('#Quantity_' + productid).val(rs.quantity);
            } else {
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function UpdateCheckQuantity(productid, quantity) {
    $.ajax({
        url: "/ShoppingCart/CheckQuantity",
        method: "post",
        data: { productId: productid, quantity: quantity },

        success: function (rs) {
            if (!rs.success) {
                if (quantity > rs.quantity) {
                    alert("Số lượng sản phẩm chỉ còn " + rs.quantity);
                    $('#Quantity_' + productid).val(rs.quantity);
                    UpdateTotalAndCart(productid, rs.quantity);

                }
                if (rs.quantity <= 0) {
                    alert("Chỉ được nhập tối tiểu 1 sản phẩm vui lòng");
                    UpdateTotalAndCart(productid,1);
                }
            } else {
               
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}


