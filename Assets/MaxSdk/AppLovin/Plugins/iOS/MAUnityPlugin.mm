//
//  MAUnityPlugin.mm
//  AppLovin MAX Unity Plugin
//

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-declarations"

#import "MAUnityAdManager.h"

#define VERSION @"8.3.1"
#define NSSTRING(_X) ( (_X != NULL) ? [NSString stringWithCString: _X encoding: NSStringEncodingConversionAllowLossy].al_stringByTrimmingWhitespace : nil)

@interface NSString (ALUtils)
@property (nonatomic, copy, readonly) NSString *al_stringByTrimmingWhitespace;
@property (assign, readonly, getter=al_isValidString) BOOL al_validString;
- (BOOL)al_isEqualToStringIgnoringCase:(nullable NSString *)otherString;
@end

@interface ALSdkInitializationConfigurationBuilder (ALUtils)
- (void)setSdkKey:(NSString *)sdkKey;
@end

// When native code plugin is implemented in .mm / .cpp file, then functions
// should be surrounded with extern "C" block to conform C function naming rules
extern "C"
{
    static NSString *const TAG = @"MAUnityPlugin";
    static NSString *const KeySdkKey = @"SdkKey";
    
    UIView* UnityGetGLView();
    
    static ALSdkInitializationConfigurationBuilder *_initConfigurationBuilder;
    static ALSdk *_sdk;
    static MAUnityAdManager *_adManager;

    static bool _isSdkInitialized = false;
    static bool _initializeSdkCalled = false;
    static bool _disableAllLogs = false;
    
    // Helper method to create C string copy
    static const char * cStringCopy(NSString *string);
    // Helper method to log errors
    void max_unity_log_uninitialized_access_error(const char *callingMethod);
    void max_unity_log_error(NSString *message);

    ALSdk *getSdk()
    {
        if ( !_sdk )
        {
            _sdk = [ALSdk shared];
        }
        
        return _sdk;
    }

    MAUnityAdManager *getAdManager()
    {
        if ( !_adManager )
        {
            _adManager = [MAUnityAdManager shared];
        }
        
        return _adManager;
    }

    ALSdkInitializationConfigurationBuilder *getInitConfigurationBuilder()
    {
        if ( !_initConfigurationBuilder )
        {
            NSString *sdkKey = [getSdk().settings.extraParameters al_stringForKey: KeySdkKey];
            _initConfigurationBuilder = [ALSdkInitializationConfiguration builderWithSdkKey: sdkKey];
        }
        
        return _initConfigurationBuilder;
    }

    bool max_unity_should_disable_all_logs()
    {
        return _disableAllLogs;
    }

    int getConsentStatusValue(NSNumber *consentStatus)
    {
        if ( consentStatus )
        {
            return consentStatus.intValue;
        }
        else
        {
            return -1;
        }
    }

    id getLocalExtraParameterValue(const char *json)
    {
        NSData *jsonData = [NSSTRING(json) dataUsingEncoding: NSUTF8StringEncoding];
        NSError *error;
        NSDictionary *jsonDict = [NSJSONSerialization JSONObjectWithData: jsonData
                                                                 options: 0
                                                                   error: &error];
        
        if ( error )
        {
            return nil;
        }
        else
        {
            return jsonDict[@"value"];
        }
    }

    NSArray<NSString *> * toStringArray(char **arrayPointer, int size)
    {
        NSMutableArray<NSString *> *array = [NSMutableArray arrayWithCapacity: size];
        for ( int i = 0; i < size; i++ )
        {
            NSString *element = NSSTRING(arrayPointer[i]);
            if ( element )
            {
                [array addObject: element];
            }
        }
        
        return array;
    }

    MASegmentCollection *getSegmentCollection(const char *collectionJson)
    {
        MASegmentCollectionBuilder *segmentCollectionBuilder = [MASegmentCollection builder];
        
        NSDictionary *jsonDict = [MAUnityAdManager deserializeParameters: [NSString stringWithUTF8String: collectionJson]];
        
        NSArray *segmentsArray = jsonDict[@"segments"];
        for (NSDictionary *segmentDict in segmentsArray)
        {
            NSNumber *key = segmentDict[@"key"];
            NSArray *valuesArray = segmentDict[@"values"];
            NSMutableArray *values = [NSMutableArray array];
            for (NSNumber *value in valuesArray)
            {
                [values addObject:value];
            }
            
            MASegment *segment = [[MASegment alloc] initWithKey:key values:values];
            [segmentCollectionBuilder addSegment:segment];
        }
        
        return [segmentCollectionBuilder build];
    }

    void _MaxSetBackgroundCallback(ALUnityBackgroundCallback backgroundCallback)
    {
        [MAUnityAdManager setUnityBackgroundCallback: backgroundCallback];
    }

    void _MaxSetSdkKey(const char *sdkKey)
    {
        if (!sdkKey) return;
        
        NSString *sdkKeyStr = [NSString stringWithUTF8String: sdkKey];
        [getInitConfigurationBuilder() setSdkKey: sdkKeyStr];
    }
    
    void _MaxInitializeSdk(const char *serializedAdUnitIdentifiers, const char *serializedMetaData)
    {
        ALSdkInitializationConfigurationBuilder *initConfigurationBuilder = getInitConfigurationBuilder();
        initConfigurationBuilder.mediationProvider = @"max";
        initConfigurationBuilder.pluginVersion = [@"Max-Unity-" stringByAppendingString: VERSION];
        initConfigurationBuilder.adUnitIdentifiers = [[NSString stringWithUTF8String: serializedAdUnitIdentifiers] componentsSeparatedByString: @","];
        
        [getSdk().settings setExtraParameterForKey: @"applovin_unity_metadata" value: NSSTRING(serializedMetaData)];
        
        ALSdkInitializationConfiguration *initConfig = [initConfigurationBuilder build];
        
        [getAdManager() initializeSdkWithConfiguration: initConfig andCompletionHandler:^(ALSdkConfiguration *configuration) {
            _isSdkInitialized = true;
        }];
        
        _initializeSdkCalled = true;
    }
    
    bool _MaxIsInitialized()
    {
        return _isSdkInitialized;
    }

    const char * _MaxGetAvailableMediatedNetworks()
    {
        NSArray<MAMediatedNetworkInfo *> *availableMediatedNetworks = [getSdk() availableMediatedNetworks];
        
        // Create array of serialized network strings
        NSMutableArray<NSDictionary<NSString *, id> *> *serializedNetworks = [NSMutableArray arrayWithCapacity: availableMediatedNetworks.count];
        for ( MAMediatedNetworkInfo *mediatedNetwork in availableMediatedNetworks )
        {
            NSDictionary<NSString *, id> *mediatedNetworkDictionary = @{@"name" : mediatedNetwork.name,
                                                                        @"adapterClassName" : mediatedNetwork.adapterClassName,
                                                                        @"adapterVersion" : mediatedNetwork.adapterVersion,
                                                                        @"sdkVersion" : mediatedNetwork.sdkVersion,
                                                                        @"initializationStatus" : @(mediatedNetwork.initializationStatus)};
            [serializedNetworks addObject: mediatedNetworkDictionary];
        }
        
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject: serializedNetworks options: 0 error: nil];
        return cStringCopy([[NSString alloc] initWithData: jsonData encoding: NSUTF8StringEncoding]);
    }
    
    void _MaxShowMediationDebugger()
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_error(@"Failed to show mediation debugger - please ensure the AppLovin MAX Unity Plugin has been initialized by calling 'MaxSdk.InitializeSdk();'!");
            return;
        }
        
        [getSdk() showMediationDebugger];
    }

    void _MaxShowCreativeDebugger()
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_error(@"Failed to show creative debugger - please ensure the AppLovin MAX Unity Plugin has been initialized by calling 'MaxSdk.InitializeSdk();'!");
            return;
        }
        
        [getSdk() showCreativeDebugger];
    }

    void _MaxSetUserId(const char *userId)
    {
        getSdk().settings.userIdentifier = NSSTRING(userId);
    }

    void _MaxSetSegmentCollection(const char *collectionJson)
    {
        if ( _initializeSdkCalled )
        {
            max_unity_log_error(@"Segment collection must be set before MAX SDK is initialized");
            return;
        }
        
        getInitConfigurationBuilder().segmentCollection = getSegmentCollection(collectionJson);
    }

    const char * _MaxGetSdkConfiguration()
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxGetSdkConfiguration");
            return cStringCopy(@"");
        }
        
        NSString *consentFlowUserGeographyStr = @(getSdk().configuration.consentFlowUserGeography).stringValue;
        NSString *consentDialogStateStr = @(getSdk().configuration.consentDialogState).stringValue;
        NSString *appTrackingStatus = @(getSdk().configuration.appTrackingTransparencyStatus).stringValue; // Deliberately name it `appTrackingStatus` to be a bit more generic (in case Android introduces a similar concept)

        return cStringCopy([MAUnityAdManager serializeParameters: @{@"consentFlowUserGeography" : consentFlowUserGeographyStr,
                                                                    @"consentDialogState" : consentDialogStateStr,
                                                                    @"countryCode" : getSdk().configuration.countryCode,
                                                                    @"appTrackingStatus" : appTrackingStatus,
                                                                    @"isSuccessfullyInitialized" : @([getSdk() isInitialized]),
                                                                    @"isTestModeEnabled" : @([getSdk().configuration isTestModeEnabled])}]);
    }
    
    void _MaxSetHasUserConsent(bool hasUserConsent)
    {
        [ALPrivacySettings setHasUserConsent: hasUserConsent];
    }
    
    bool _MaxHasUserConsent()
    {
        return [ALPrivacySettings hasUserConsent];
    }
    
    bool _MaxIsUserConsentSet()
    {
        return [ALPrivacySettings isUserConsentSet];
    }

    void _MaxSetDoNotSell(bool doNotSell)
    {
        [ALPrivacySettings setDoNotSell: doNotSell];
    }
    
    bool _MaxIsDoNotSell()
    {
        return [ALPrivacySettings isDoNotSell];
    }

    bool _MaxIsDoNotSellSet()
    {
        return [ALPrivacySettings isDoNotSellSet];
    }
    
    void _MaxCreateBanner(const char *adUnitIdentifier, const char *bannerPosition, bool isAdaptive)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxCreateBanner");
            return;
        }
        
        [getAdManager() createBannerWithAdUnitIdentifier: NSSTRING(adUnitIdentifier) atPosition: NSSTRING(bannerPosition) isAdaptive: isAdaptive];
    }

    void _MaxCreateBannerXY(const char *adUnitIdentifier, const float x, const float y, bool isAdaptive)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxCreateBannerXY");
            return;
        }
        
        [getAdManager() createBannerWithAdUnitIdentifier: NSSTRING(adUnitIdentifier) x: x y: y isAdaptive: isAdaptive];
    }
    
   void _MaxLoadBanner(const char *adUnitIdentifier)
   {
       if ( !_initializeSdkCalled )
       {
           max_unity_log_uninitialized_access_error("_MaxLoadBanner");
           return;
       }
       
       [getAdManager() loadBannerWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
   }
    
    void _MaxSetBannerBackgroundColor(const char *adUnitIdentifier, const char *hexColorCode)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetBannerBackgroundColor");
            return;
        }
        
        [getAdManager() setBannerBackgroundColorForAdUnitIdentifier: NSSTRING(adUnitIdentifier) hexColorCode: NSSTRING(hexColorCode)];
    }
    
    void _MaxSetBannerPlacement(const char *adUnitIdentifier, const char *placement)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetBannerPlacement");
            return;
        }
        
        [getAdManager() setBannerPlacement: NSSTRING(placement) forAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxStartBannerAutoRefresh(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxStartBannerAutoRefresh");
            return;
        }
        
        [getAdManager() startBannerAutoRefreshForAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxStopBannerAutoRefresh(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxStopBannerAutoRefresh");
            return;
        }
        
        [getAdManager() stopBannerAutoRefreshForAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxSetBannerExtraParameter(const char *adUnitIdentifier, const char *key, const char *value)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetBannerExtraParameter");
            return;
        }
        
        [getAdManager() setBannerExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                               key: NSSTRING(key)
                                                             value: NSSTRING(value)];
    }
    
    void _MaxSetBannerLocalExtraParameter(const char *adUnitIdentifier, const char *key, MAUnityRef value)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetBannerLocalExtraParameter");
            return;
        }
        
        [getAdManager() setBannerLocalExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                                    key: NSSTRING(key)
                                                                  value: (__bridge id) value];
    }

    void _MaxSetBannerLocalExtraParameterJSON(const char *adUnitIdentifier, const char *key, const char *json)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetBannerLocalExtraParameter");
            return;
        }
        
        id value = getLocalExtraParameterValue(json);
        [getAdManager() setBannerLocalExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                                    key: NSSTRING(key)
                                                                  value: value];
    }
    
    void _MaxSetBannerCustomData(const char *adUnitIdentifier, const char *customData)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetBannerCustomData");
            return;
        }
        
        [getAdManager() setBannerCustomData: NSSTRING(customData) forAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }

    void _MaxSetBannerWidth(const char *adUnitIdentifier, const float width)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetBannerWidth");
            return;
        }
        
        [getAdManager() setBannerWidth: width forAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxUpdateBannerPosition(const char *adUnitIdentifier, const char *bannerPosition)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxUpdateBannerPosition");
            return;
        }
        
        [getAdManager() updateBannerPosition: NSSTRING(bannerPosition) forAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }

    void _MaxUpdateBannerPositionXY(const char *adUnitIdentifier, const float x, const float y)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxUpdateBannerPositionXY");
            return;
        }
        
        [getAdManager() updateBannerPosition: x y: y forAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxShowBanner(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxShowBanner");
            return;
        }
        
        [getAdManager() showBannerWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxDestroyBanner(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxDestroyBanner");
            return;
        }
        
        [getAdManager() destroyBannerWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxHideBanner(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxHideBanner");
            return;
        }
        
        [getAdManager() hideBannerWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    const char * _MaxGetBannerLayout(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxGetBannerLayout");
            return cStringCopy(@"");
        }
                
        return cStringCopy([getAdManager() bannerLayoutForAdUnitIdentifier: NSSTRING(adUnitIdentifier)]);
    }
    
    void _MaxCreateMRec(const char *adUnitIdentifier, const char *mrecPosition)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxCreateMRec");
            return;
        }
        
        [getAdManager() createMRecWithAdUnitIdentifier: NSSTRING(adUnitIdentifier) atPosition: NSSTRING(mrecPosition)];
    }
    
    void _MaxCreateMRecXY(const char *adUnitIdentifier, const float x, const float y)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxCreateMRecXY");
            return;
        }
        
        [getAdManager() createMRecWithAdUnitIdentifier: NSSTRING(adUnitIdentifier) x: x y: y];
    }
    
   void _MaxLoadMRec(const char *adUnitIdentifier)
   {
       if ( !_initializeSdkCalled )
       {
           max_unity_log_uninitialized_access_error("_MaxLoadMRec");
           return;
       }
       
       [getAdManager() loadMRecWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
   }
    
    void _MaxSetMRecPlacement(const char *adUnitIdentifier, const char *placement)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetMRecPlacement");
            return;
        }
        
        [getAdManager() setMRecPlacement: NSSTRING(placement) forAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxStartMRecAutoRefresh(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxStartMRecAutoRefresh");
            return;
        }
        
        [getAdManager() startMRecAutoRefreshForAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxStopMRecAutoRefresh(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxStopMRecAutoRefresh");
            return;
        }
        
        [getAdManager() stopMRecAutoRefreshForAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxUpdateMRecPosition(const char *adUnitIdentifier, const char *mrecPosition)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxUpdateMRecPosition");
            return;
        }
        
        [getAdManager() updateMRecPosition: NSSTRING(mrecPosition) forAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxUpdateMRecPositionXY(const char *adUnitIdentifier, const float x, const float y)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxUpdateMRecPositionXY");
            return;
        }
        
        [getAdManager() updateMRecPosition: x y: y forAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxShowMRec(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxShowMRec");
            return;
        }
        
        [getAdManager() showMRecWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxDestroyMRec(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxDestroyMRec");
            return;
        }
        
        [getAdManager() destroyMRecWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxHideMRec(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxHideMRec");
            return;
        }
        
        [getAdManager() hideMRecWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }

    void _MaxSetMRecExtraParameter(const char *adUnitIdentifier, const char *key, const char *value)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetMRecExtraParameter");
            return;
        }
        
        [getAdManager() setMRecExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                         key: NSSTRING(key)
                                                       value: NSSTRING(value)];
    }
    
    void _MaxSetMRecLocalExtraParameter(const char *adUnitIdentifier, const char *key, MAUnityRef value)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetMRecLocalExtraParameter");
            return;
        }
        
        [getAdManager() setMRecLocalExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                                  key: NSSTRING(key)
                                                                value: (__bridge id)value];
    }

    void _MaxSetMRecLocalExtraParameterJSON(const char *adUnitIdentifier, const char *key, const char *json)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetMRecLocalExtraParameter");
            return;
        }
        
        id value = getLocalExtraParameterValue(json);
        [getAdManager() setMRecLocalExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                                  key: NSSTRING(key)
                                                                value: value];
    }
    
    void _MaxSetMRecCustomData(const char *adUnitIdentifier, const char *customData)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetMRecCustomData");
            return;
        }
        
        [getAdManager() setMRecCustomData: NSSTRING(customData) forAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }

    const char * _MaxGetMRecLayout(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxGetMRecLayout");
            return cStringCopy(@"");
        }
                
        return cStringCopy([getAdManager() mrecLayoutForAdUnitIdentifier: NSSTRING(adUnitIdentifier)]);
    }
    
    void _MaxLoadInterstitial(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxLoadInterstitial");
            return;
        }
        
        [getAdManager() loadInterstitialWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxSetInterstitialExtraParameter(const char *adUnitIdentifier, const char *key, const char *value)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetInterstitialExtraParameter");
            return;
        }
        
        [getAdManager() setInterstitialExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                                     key: NSSTRING(key)
                                                                   value: NSSTRING(value)];
    }
    
    void _MaxSetInterstitialLocalExtraParameter(const char *adUnitIdentifier, const char *key, MAUnityRef value)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetInterstitialLocalExtraParameter");
            return;
        }
        
        [getAdManager() setInterstitialLocalExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                                          key: NSSTRING(key)
                                                                        value: (__bridge id)value];
    }

    void _MaxSetInterstitialLocalExtraParameterJSON(const char *adUnitIdentifier, const char *key, const char *json)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetInterstitialLocalExtraParameter");
            return;
        }
        
        id value = getLocalExtraParameterValue(json);
        [getAdManager() setInterstitialLocalExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                                          key: NSSTRING(key)
                                                                        value: value];
    }

    bool _MaxIsInterstitialReady(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxIsInterstitialReady");
            return false;
        }
        
        return [getAdManager() isInterstitialReadyWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxShowInterstitial(const char *adUnitIdentifier, const char *placement, const char *customData)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxShowInterstitial");
            return;
        }
        
        [getAdManager() showInterstitialWithAdUnitIdentifier: NSSTRING(adUnitIdentifier) placement: NSSTRING(placement) customData: NSSTRING(customData)];
    }
    
    void _MaxLoadAppOpenAd(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxLoadAppOpenAd");
            return;
        }
        
        [getAdManager() loadAppOpenAdWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxSetAppOpenAdExtraParameter(const char *adUnitIdentifier, const char *key, const char *value)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetAppOpenAdExtraParameter");
            return;
        }
        
        [getAdManager() setAppOpenAdExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                                  key: NSSTRING(key)
                                                                value: NSSTRING(value)];
    }
    
    void _MaxSetAppOpenAdLocalExtraParameter(const char *adUnitIdentifier, const char *key, MAUnityRef value)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetAppOpenAdLocalExtraParameter");
            return;
        }
        
        [getAdManager() setAppOpenAdLocalExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                                       key: NSSTRING(key)
                                                                     value: (__bridge id)value];
    }

    void _MaxSetAppOpenAdLocalExtraParameterJSON(const char *adUnitIdentifier, const char *key, const char *json)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetAppOpenAdLocalExtraParameter");
            return;
        }
        
        id value = getLocalExtraParameterValue(json);
        [getAdManager() setAppOpenAdLocalExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                                       key: NSSTRING(key)
                                                                     value: value];
    }
    
    bool _MaxIsAppOpenAdReady(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxIsAppOpenAdReady");
            return false;
        }
        
        return [getAdManager() isAppOpenAdReadyWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxShowAppOpenAd(const char *adUnitIdentifier, const char *placement, const char *customData)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxShowAppOpenAd");
            return;
        }
        
        [getAdManager() showAppOpenAdWithAdUnitIdentifier: NSSTRING(adUnitIdentifier) placement: NSSTRING(placement) customData: NSSTRING(customData)];
    }
    
    void _MaxLoadRewardedAd(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxLoadRewardedAd");
            return;
        }
        
        [getAdManager() loadRewardedAdWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxSetRewardedAdExtraParameter(const char *adUnitIdentifier, const char *key, const char *value)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetRewardedAdExtraParameter");
            return;
        }
        
        [getAdManager() setRewardedAdExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                                   key: NSSTRING(key)
                                                                 value: NSSTRING(value)];
    }
    
    void _MaxSetRewardedAdLocalExtraParameter(const char *adUnitIdentifier, const char *key, MAUnityRef value)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetRewardedAdLocalExtraParameter");
            return;
        }
        
        [getAdManager() setRewardedAdLocalExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                                        key: NSSTRING(key)
                                                                      value: (__bridge id)value];
    }

    void _MaxSetRewardedAdLocalExtraParameterJSON(const char *adUnitIdentifier, const char *key, const char *json)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxSetRewardedAdLocalExtraParameter");
            return;
        }
        
        id value = getLocalExtraParameterValue(json);
        [getAdManager() setRewardedAdLocalExtraParameterForAdUnitIdentifier: NSSTRING(adUnitIdentifier)
                                                                        key: NSSTRING(key)
                                                                      value: value];
    }

    bool _MaxIsRewardedAdReady(const char *adUnitIdentifier)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxIsRewardedAdReady");
            return false;
        }
        
        return [getAdManager() isRewardedAdReadyWithAdUnitIdentifier: NSSTRING(adUnitIdentifier)];
    }
    
    void _MaxShowRewardedAd(const char *adUnitIdentifier, const char *placement, const char *customData)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxShowRewardedAd");
            return;
        }
        
        [getAdManager() showRewardedAdWithAdUnitIdentifier: NSSTRING(adUnitIdentifier) placement: NSSTRING(placement) customData: NSSTRING(customData)];
    }
    
    void _MaxTrackEvent(const char *event, const char *parameters)
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxTrackEvent");
            return;
        }
        
        [getAdManager() trackEvent: NSSTRING(event) parameters: NSSTRING(parameters)];
    }
    
    bool _MaxIsTablet()
    {
        return [UIDevice currentDevice].userInterfaceIdiom == UIUserInterfaceIdiomPad;
    }

    bool _MaxIsPhysicalDevice()
    {
        return !ALUtils.simulator;
    }

    int _MaxGetTcfVendorConsentStatus(int vendorIdentifier)
    {
        NSNumber *consentStatus = [ALPrivacySettings tcfVendorConsentStatusForIdentifier: vendorIdentifier];
        return getConsentStatusValue(consentStatus);
    }

    int _MaxGetAdditionalConsentStatus(int atpIdentifier)
    {
        NSNumber *consentStatus = [ALPrivacySettings additionalConsentStatusForIdentifier: atpIdentifier];
        return getConsentStatusValue(consentStatus);
    }

    int _MaxGetPurposeConsentStatus(int purposeIdentifier)
    {
        NSNumber *consentStatus = [ALPrivacySettings purposeConsentStatusForIdentifier: purposeIdentifier];
        return getConsentStatusValue(consentStatus);
    }

    int _MaxGetSpecialFeatureOptInStatus(int specialFeatureIdentifier)
    {
        NSNumber *consentStatus = [ALPrivacySettings specialFeatureOptInStatusForIdentifier: specialFeatureIdentifier];
        return getConsentStatusValue(consentStatus);
    }
    
    static const char * cStringCopy(NSString *string)
    {
        const char *value = string.UTF8String;
        return value ? strdup(value) : NULL;
    }
    
    void _MaxSetMuted(bool muted)
    {
        getSdk().settings.muted = muted;
    }
    
    bool _MaxIsMuted()
    {
        return getSdk().settings.muted;
    }
    
    float _MaxScreenDensity()
    {
        return [UIScreen.mainScreen nativeScale];
    }
    
    const char * _MaxGetAdValue(const char *adUnitIdentifier, const char *key)
    {
        return cStringCopy([getAdManager() adValueForAdUnitIdentifier: NSSTRING(adUnitIdentifier) withKey: NSSTRING(key)]);
    }

    void _MaxSetVerboseLogging(bool enabled)
    {
        getSdk().settings.verboseLoggingEnabled = enabled;
    }
    
    bool _MaxIsVerboseLoggingEnabled()
    {
        return [getSdk().settings isVerboseLoggingEnabled];
    }

    void _MaxSetTestDeviceAdvertisingIdentifiers(char **advertisingIdentifiers, int size)
    {
        if ( _initializeSdkCalled )
        {
            max_unity_log_error(@"Test device advertising IDs must be set before MAX SDK is initialized");
            return;
        }
        
        NSArray<NSString *> *advertisingIdentifiersArray = toStringArray(advertisingIdentifiers, size);
        getInitConfigurationBuilder().testDeviceAdvertisingIdentifiers = advertisingIdentifiersArray;
    }

    void _MaxSetCreativeDebuggerEnabled(bool enabled)
    {
        getSdk().settings.creativeDebuggerEnabled = enabled;
    }
    
    void _MaxSetExceptionHandlerEnabled(bool enabled)
    {
        if ( _initializeSdkCalled )
        {
            max_unity_log_error(@"Exception handler must be enabled/disabled before MAX SDK is initialized");
            return;
        }
        
        getInitConfigurationBuilder().exceptionHandlerEnabled = enabled;
    }
    
    void _MaxSetExtraParameter(const char *key, const char *value)
    {
        NSString *stringKey = NSSTRING(key);
        if ( ![stringKey al_isValidString] )
        {
            NSString *message = [NSString stringWithFormat:@"Failed to set extra parameter for nil or empty key: %@", stringKey];
            max_unity_log_error(message);
            return;
        }
        
        NSString *stringValue = NSSTRING(value);
        if ( [@"disable_all_logs" isEqualToString: stringKey] )
        {
            _disableAllLogs = [@"true" al_isEqualToStringIgnoringCase: stringValue];
        }
        
        ALSdkSettings *settings = getSdk().settings;
        [settings setExtraParameterForKey: stringKey value: stringValue];
    }

    int * _MaxGetSafeAreaInsets()
    {
        UIEdgeInsets safeAreaInsets = UnityGetGLView().safeAreaInsets;
        static int insets[4] = {(int) safeAreaInsets.left, (int) safeAreaInsets.top, (int) safeAreaInsets.right, (int) safeAreaInsets.bottom};
        return insets;
    }
    
    void _MaxShowCmpForExistingUser()
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxShowCmpForExistingUser");
            return;
        }
        
        [getAdManager() showCMPForExistingUser];
    }
    
    bool _MaxHasSupportedCmp()
    {
        if ( !_initializeSdkCalled )
        {
            max_unity_log_uninitialized_access_error("_MaxHasSupportedCmp");
            return false;
        }
        
        return [getSdk().cmpService hasSupportedCMP];
    }

    float _MaxGetAdaptiveBannerHeight(const float width)
    {
        return [MAUnityAdManager adaptiveBannerHeightForWidth: width];
    }

    void max_unity_log_uninitialized_access_error(const char *callingMethod)
    {
        NSString *message = [NSString stringWithFormat:@"Failed to execute: %s - please ensure the AppLovin MAX Unity Plugin has been initialized by calling 'MaxSdk.InitializeSdk();'!", callingMethod];
        max_unity_log_error(message);
    }

    void max_unity_log_error(NSString *message)
    {
        if (_disableAllLogs) return;
        
        NSString *logMessage = [NSString stringWithFormat: @"[%@] %@", TAG, message];
        NSLog(@"%@", logMessage);
    }
}

#pragma clang diagnostic pop
