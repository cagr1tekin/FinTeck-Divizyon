// İnteraktif Kredi - Web Şube 2.0
// Ana JavaScript dosyası

(function($) {
    'use strict';

    // Document Ready
    $(document).ready(function() {
        init_app();
    });

    // Uygulama başlatma
    function init_app() {
        init_form_validation();
        init_input_masking();
        init_double_submit_prevention();
        init_ajax_helpers();
        init_global_loading();
        init_session_timeout();
    }

    // Form validation helper'ları
    function init_form_validation() {
        // Client-side validation için jQuery Validation kullanılacak
        // Server-side validation zaten Razor Pages tarafından yapılıyor
    }

    // Input masking fonksiyonları
    function init_input_masking() {
        // TCKN maskeleme (5XX...XX formatı)
        $(document).on('input', 'input[data-mask="tckn"]', function() {
            var $input = $(this);
            var value = $input.val().replace(/\D/g, ''); // Sadece rakam
            if (value.length > 11) value = value.substring(0, 11);
            $input.val(value);
        });

        // Telefon maskeleme
        $(document).on('input', 'input[data-mask="phone"]', function() {
            var $input = $(this);
            var value = $input.val().replace(/\D/g, ''); // Sadece rakam
            if (value.length > 10) value = value.substring(0, 10);
            $input.val(value);
        });

        // Para formatı
        $(document).on('blur', 'input[data-mask="currency"]', function() {
            var $input = $(this);
            var value = parseFloat($input.val()) || 0;
            $input.val(value.toFixed(2));
        });
    }

    // Double submit prevention
    function init_double_submit_prevention() {
        $(document).on('submit', 'form', function(e) {
            var $form = $(this);
            var $btn = $form.find('button[type="submit"], input[type="submit"]');

            // Zaten gönderiliyorsa engelle
            if ($form.data('submitting')) {
                e.preventDefault();
                return false;
            }

            // Gönderim işaretle ve butonu kilitle
            $form.data('submitting', true);
            $btn.prop('disabled', true)
                .addClass('btn_primary--loading')
                .each(function() {
                    var $this = $(this);
                    $this.data('original-text', $this.text());
                    $this.text('İşleniyor...');
                });
        });
    }

    // AJAX helper fonksiyonları
    function init_ajax_helpers() {
        // Global AJAX setup - Loading spinner
        $(document).ajaxStart(function() {
            show_global_loading();
        });

        $(document).ajaxStop(function() {
            hide_global_loading();
        });

        // Global AJAX error handler
        $(document).ajaxError(function(event, xhr, settings, thrownError) {
            hide_global_loading();
            
            // 401 Unauthorized - Session timeout
            if (xhr.status === 401) {
                show_toast('Oturumunuz sona erdi. Lütfen tekrar giriş yapın.', 'warning');
                setTimeout(function() {
                    window.location.href = '/Onboarding/TcknGsm';
                }, 2000);
                return;
            }

            // 403 Forbidden
            if (xhr.status === 403) {
                show_toast('Bu işlem için yetkiniz bulunmamaktadır.', 'error');
                return;
            }

            // 404 Not Found
            if (xhr.status === 404) {
                show_toast('İstenen kaynak bulunamadı.', 'error');
                return;
            }

            // 500 Internal Server Error
            if (xhr.status === 500) {
                show_toast('Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin.', 'error');
                return;
            }

            // Genel hata
            show_toast('Bir hata oluştu. Lütfen tekrar deneyin.', 'error');
            console.error('AJAX Error:', thrownError);
        });
    }

    // Global loading spinner
    function init_global_loading() {
        // Loading overlay oluştur
        var $loading = $('<div>', {
            id: 'global_loading',
            class: 'global_loading',
            'aria-label': 'Yükleniyor',
            'aria-live': 'polite'
        });

        var $spinner = $('<div>', {
            class: 'global_loading__spinner'
        });

        var $text = $('<p>', {
            class: 'global_loading__text',
            text: 'Yükleniyor...'
        });

        $loading.append($spinner);
        $loading.append($text);
        $('body').append($loading);
    }

    window.show_global_loading = function() {
        $('#global_loading').addClass('global_loading--visible');
    };

    window.hide_global_loading = function() {
        $('#global_loading').removeClass('global_loading--visible');
    };

    // Session timeout handling
    function init_session_timeout() {
        var sessionTimeout = 30 * 60 * 1000; // 30 dakika (milisaniye)
        var warningTime = 5 * 60 * 1000; // 5 dakika önceden uyarı
        var lastActivity = Date.now();
        var warningShown = false;

        // Kullanıcı aktivitesini takip et
        var activityEvents = ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click'];
        activityEvents.forEach(function(event) {
            $(document).on(event, function() {
                lastActivity = Date.now();
                warningShown = false;
            });
        });

        // Session timeout kontrolü
        setInterval(function() {
            var timeSinceActivity = Date.now() - lastActivity;
            var timeUntilTimeout = sessionTimeout - timeSinceActivity;

            // Uyarı zamanı geldi mi?
            if (timeUntilTimeout <= warningTime && timeUntilTimeout > 0 && !warningShown) {
                warningShown = true;
                show_toast('Oturumunuz ' + Math.ceil(timeUntilTimeout / 60000) + ' dakika içinde sona erecek. Lütfen işlemlerinizi tamamlayın.', 'warning', 10000);
            }

            // Session timeout oldu mu?
            if (timeUntilTimeout <= 0) {
                show_toast('Oturumunuz sona erdi. Lütfen tekrar giriş yapın.', 'warning');
                setTimeout(function() {
                    window.location.href = '/Onboarding/TcknGsm';
                }, 2000);
            }
        }, 60000); // Her dakika kontrol et
    }

    // TCKN Maskeleme (5XX...XX formatı)
    window.mask_tckn = function(tckn) {
        if (!tckn || tckn.length < 11) return tckn;
        return tckn.substring(0, 1) + '*'.repeat(8) + tckn.substring(9);
    };

    // Telefon Maskeleme
    window.mask_phone = function(phone) {
        if (!phone || phone.length < 10) return phone;
        return phone.substring(0, 3) + '****' + phone.substring(7);
    };

    // Para Formatı (10000 -> 10.000 ₺)
    window.format_currency = function(amount) {
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: 'TRY',
            minimumFractionDigits: 0
        }).format(amount);
    };

    // Toast bildirimi göster
    window.show_toast = function(message, type, duration) {
        type = type || 'info';
        duration = duration || 3000;

        // Toast container'ı oluştur (yoksa)
        var $container = $('.toast_container');
        if ($container.length === 0) {
            $container = $('<div>', {
                class: 'toast_container'
            });
            $('body').append($container);
        }

        // Toast elementi oluştur
        var $toast = $('<div>', {
            class: 'toast toast--' + type
        });

        // Icon
        var $icon = $('<div>', {
            class: 'toast__icon'
        });

        // Message
        var $message = $('<div>', {
            class: 'toast__message',
            text: message
        });

        // Close button
        var $close = $('<button>', {
            class: 'toast__close',
            type: 'button',
            'aria-label': 'Kapat'
        });

        // Close button click handler
        $close.on('click', function() {
            hide_toast($toast);
        });

        // Progress bar (auto-dismiss için)
        var $progress = $('<div>', {
            class: 'toast__progress'
        });

        // Toast yapısını oluştur
        $toast.append($icon);
        $toast.append($message);
        $toast.append($close);
        $toast.append($progress);

        // Container'a ekle
        $container.append($toast);

        // Animasyon için kısa gecikme
        setTimeout(function() {
            $toast.addClass('toast--visible');
        }, 10);

        // Progress bar animasyonu
        $progress.css('animation-duration', (duration / 1000) + 's');

        // Auto-dismiss (duration > 0 ise)
        if (duration > 0) {
            setTimeout(function() {
                hide_toast($toast);
            }, duration);
        }

        // Toast gizleme fonksiyonu
        function hide_toast($toast_element) {
            $toast_element.removeClass('toast--visible');
            setTimeout(function() {
                $toast_element.remove();
                // Container boşsa kaldır
                if ($container.children().length === 0) {
                    $container.remove();
                }
            }, 300);
        }
    };

    // API çağrısı helper
    window.api_call = function(url, method, data, success_callback, error_callback) {
        var $btn = $('button[type="submit"]');
        var original_text = $btn.text();

        $btn.prop('disabled', true).text('İşleniyor...');

        $.ajax({
            url: url,
            method: method || 'POST',
            contentType: 'application/json',
            data: data ? JSON.stringify(data) : null,
            success: function(response) {
                if (response.success) {
                    if (success_callback) success_callback(response);
                } else {
                    show_toast(response.message || 'İşlem başarısız.', 'error');
                    if (error_callback) error_callback(response);
                }
            },
            error: function(xhr, status, error) {
                show_toast('Bir hata oluştu. Lütfen tekrar deneyin.', 'error');
                if (error_callback) error_callback(xhr, status, error);
            },
            complete: function() {
                $btn.prop('disabled', false).text(original_text);
            }
        });
    };

    // Müşteri doğrulama fonksiyonu
    window.validate_customer = function(tckn, gsm) {
        var $btn = $('#validate_btn');
        var original_text = $btn.text();

        $btn.prop('disabled', true).text('Doğrulanıyor...');

        api_call(
            '/api/customer/tckn-gsm',
            'POST',
            { TCKN: tckn, GSM: gsm },
            function(response) {
                show_toast('Doğrulama başarılı!', 'success');
                // Sonraki adıma geç
            },
            function() {
                // Error callback
            }
        );
    };

})(jQuery);

