// SignalR Connection Manager - Single instance management
(function () {
    'use strict';

    // Don't initialize if already exists
    if (window.SignalRManager) {
        console.log('SignalRManager already exists');
        return;
    }

    // Wait for dependencies before creating the manager
    function waitForDependencies(callback) {
        var attempts = 0;
        var maxAttempts = 40; // 20 seconds max wait
        
        function checkDependencies() {
            attempts++;
            
            if (typeof $ !== 'undefined' && typeof $.connection !== 'undefined') {
                console.log('Dependencies loaded, creating SignalRManager');
                callback();
            } else if (attempts < maxAttempts) {
                setTimeout(checkDependencies, 500);
            } else {
                console.error('SignalR dependencies not loaded after 20 seconds');
            }
        }
        
        checkDependencies();
    }

    // Create the manager only when dependencies are ready
    waitForDependencies(function() {
        window.SignalRManager = {
            connection: null,
            isConnecting: false,
            connectionPromise: null,
            initialized: false,
            
            // Initialize connection if not already done
            init: function() {
                var self = this;
                
                if (this.initialized && this.connection && this.connection.state === $.signalR.connectionState.connected) {
                    console.log('SignalR already connected');
                    return Promise.resolve(this.connection);
                }
                
                if (this.isConnecting && this.connectionPromise) {
                    console.log('SignalR connection in progress');
                    return this.connectionPromise;
                }
                
                this.isConnecting = true;
                console.log('Starting SignalR connection...');
                
                try {
                    // Configure connection events
                    $.connection.hub.connectionSlow(function () {
                        console.log('SignalR connection is slow');
                    });
                    
                    $.connection.hub.reconnecting(function () {
                        console.log('SignalR reconnecting...');
                    });
                    
                    $.connection.hub.reconnected(function () {
                        console.log('SignalR reconnected');
                    });
                    
                    $.connection.hub.disconnected(function () {
                        console.log('SignalR disconnected');
                        self.initialized = false;
                        self.isConnecting = false;
                        
                        // Auto-reconnect after 5 seconds
                        setTimeout(function() {
                            if ($.connection.hub.state === $.signalR.connectionState.disconnected) {
                                console.log('Attempting auto-reconnect...');
                                self.init();
                            }
                        }, 5000);
                    });
                    
                    // Start connection
                    this.connectionPromise = $.connection.hub.start({
                        transport: ['webSockets', 'serverSentEvents', 'longPolling'],
                        waitForPageLoad: false
                    }).done(function () {
                        console.log('SignalR connection established successfully');
                        self.connection = $.connection.hub;
                        self.isConnecting = false;
                        self.initialized = true;
                    }).fail(function (error) {
                        console.error('SignalR connection failed:', error);
                        self.isConnecting = false;
                        self.initialized = false;
                    });
                    
                    return this.connectionPromise;
                } catch (error) {
                    console.error('Error initializing SignalR:', error);
                    this.isConnecting = false;
                    return Promise.reject(error);
                }
            },
            
            // Get current connection state
            getState: function() {
                return this.connection ? this.connection.state : 'disconnected';
            },
            
            // Check if connection is ready
            isReady: function() {
                return this.initialized && this.connection && this.connection.state === $.signalR.connectionState.connected;
            },
            
            // Disconnect and cleanup
            disconnect: function() {
                if (this.connection) {
                    this.connection.stop();
                    this.connection = null;
                    this.isConnecting = false;
                    this.connectionPromise = null;
                    this.initialized = false;
                }
            }
        };

        console.log('SignalRManager created successfully');
        
        // Auto-initialize after a short delay
        setTimeout(function() {
            if (window.SignalRManager) {
                window.SignalRManager.init();
            }
        }, 1000);
    });

})();