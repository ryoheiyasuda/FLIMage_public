/*============================================================================*/
/*                        L a b W i n d o w s / C V I                         */
/*----------------------------------------------------------------------------*/
/*    Copyright (c) National Instruments 1987-2015.  All Rights Reserved.     */
/*----------------------------------------------------------------------------*/
/*                                                                            */
/* Title:       cvidef.h                                                      */
/* Purpose:     To define macros for conditional header processing.           */
/*                                                                            */
/*============================================================================*/

#ifndef _CVIDEF_H
#define _CVIDEF_H

#ifdef __cplusplus
	extern "C" {
#endif

/*
 * Make all possible predefined macros agree
 */
#if (!defined(_NI_mswin32_)) && (defined(_NI_mswin64_)||defined(__WIN64__)||defined(_WIN64)||defined(WIN64))
    #ifndef WIN64
        #define WIN64 1
    #endif
    #ifndef WIN32
        #define WIN32 1
    #endif
    #ifndef _NI_mswin64_
        #define _NI_mswin64_ 1
    #endif
#endif
#if (!defined(_NI_mswin16_) && !defined(_NI_mswin64_)) && (defined(_NI_mswin32_)||defined(__WIN32__)||defined(__NT__)||defined(_WIN32)||defined(WIN32))
    #ifndef WIN32
        #define WIN32 1
    #endif
    #ifndef _NI_mswin32_
        #define _NI_mswin32_ 1
    #endif
#endif
#if defined(_NI_mswin16_) || defined(_NI_mswin32_) || defined(_NI_mswin64_)
    #ifndef _NI_i386_
        #define _NI_i386_ 1
    #endif
    #ifndef _NI_mswin_
        #define _NI_mswin_ 1
    #endif
#endif

/* Other compilers may define __sparc or sparc */
#if defined(__sparc)

    #if !defined(_NI_sparc_)
        #ifdef __SVR4
            #define _NI_sparc_  2   /* SunOS 5.x */
        #else
            #define _NI_sparc_  1   /* SunOS 4.x */
        #endif
    #endif

#elif defined(sparc) && sparc
    #if !defined(_NI_sparc_)
		#define _NI_sparc_	1 /* assume SunOS 4.x */
    #endif
#endif


#if defined(_NI_sparc_) && _NI_sparc_ == 2
    #if !defined(__sparc)
        #define __sparc /* used by some headers on SunOS 5.x (sys/isa_defs.h) */
    #endif
#endif

#if defined(__hp9000s700) || defined(__hppa__) || defined(__hpux)
    #if !defined(_NI_hpux_)
        #define _NI_hpux_ 1
    #endif
#endif

#if defined(__linux__)
    #if !defined(_NI_linux_)
        #define _NI_linux_ 1
    #endif
#endif

#if defined(_NI_sparc_) || defined(_NI_hpux_) || defined(_NI_linux_)
    #if !defined(_NI_unix_)
        #define _NI_unix_ 1
    #endif
#endif

/*
 * Begining of WIN32 specific defines
 */
#if defined(_NI_mswin32_)|| defined(_NI_mswin64_)

    /* This is needed for borland 4.52. Starting with borland 5.0   */
    /* this is no longer necessary. The macro is used by instrument */
    /* drivers.                                                     */
    #if defined(__BORLANDC__) &&  (__BORLANDC__ < 0x500)
        #define DLLIMPORT __import
        #define DLLEXPORT __export
    #else
        #define DLLIMPORT __declspec(dllimport)
        #define DLLEXPORT __declspec(dllexport)
    #endif

	/* This is here for external compilers. In CVI (as of version 5.0) */
	/* _CVI_USE_FUNCS_FOR_VARS_ is a predefined macro.                 */
	#if !defined(_CVI_USE_FUNCS_FOR_VARS_) && !defined(_CVI_DEBUG_)
		#define _CVI_USE_FUNCS_FOR_VARS_	1
	#endif

    #define CVICDECL __cdecl

    /* CVICALLBACK is used for typdefs for ptrs to functions. For example:
     *  typedef void (CVICALLBACK * MenuDimmerCallbackPtr)(int menuBar,  int panel);
     */
    #define CVICALLBACK     CVICDECL

    /* for function prototypes */
    #define CVIFUNC         __stdcall
    #define CVIFUNC_C       CVICDECL
    #define CVIANSI         CVICDECL

    #define CVIFUNCPTRVAR   __stdcall
    #define DLLSTDCALL      __stdcall


    #ifndef CVI_EXCLUDE_WINDOWS_TYPES
        /*****************************************************************/
        /* Constants from winnt.h for the fdwReason parameter of DllMain */
        /*****************************************************************/
        #ifndef DLL_PROCESS_ATTACH
        #define DLL_PROCESS_ATTACH 1
        #endif
        #ifndef DLL_THREAD_ATTACH
        #define DLL_THREAD_ATTACH  2
        #endif
        #ifndef DLL_THREAD_DETACH
        #define DLL_THREAD_DETACH  3
        #endif
        #ifndef DLL_PROCESS_DETACH
        #define DLL_PROCESS_DETACH 0
        #endif

        /*****************************************************************/
        /* Constants from winbase.h for the multithreading utilities.    */
        /*****************************************************************/
        #if !defined (_WINDEF_) && !defined (_WINDOWS_) /* a little safer (no warnings) */
			#ifndef THREAD_BASE_PRIORITY_LOWRT
			#define THREAD_BASE_PRIORITY_LOWRT  15
			#endif
			#ifndef THREAD_BASE_PRIORITY_MAX
			#define THREAD_BASE_PRIORITY_MAX    2
			#endif
			#ifndef THREAD_BASE_PRIORITY_MIN
			#define THREAD_BASE_PRIORITY_MIN    (-2)
			#endif
			#ifndef THREAD_BASE_PRIORITY_IDLE
			#define THREAD_BASE_PRIORITY_IDLE   (-15)
			#endif
		#endif

        #ifndef THREAD_PRIORITY_TIME_CRITICAL
		#define THREAD_PRIORITY_TIME_CRITICAL THREAD_BASE_PRIORITY_LOWRT
		#endif
		#ifndef THREAD_PRIORITY_HIGHEST
		#define THREAD_PRIORITY_HIGHEST       THREAD_BASE_PRIORITY_MAX
		#endif
		#ifndef THREAD_PRIORITY_ABOVE_NORMAL
		#define THREAD_PRIORITY_ABOVE_NORMAL  (THREAD_PRIORITY_HIGHEST-1)
		#endif
		#ifndef THREAD_PRIORITY_NORMAL
		#define THREAD_PRIORITY_NORMAL        0
		#endif
		#ifndef THREAD_PRIORITY_BELOW_NORMAL
		#define THREAD_PRIORITY_BELOW_NORMAL (THREAD_PRIORITY_LOWEST+1)
		#endif
		#ifndef THREAD_PRIORITY_LOWEST
		#define THREAD_PRIORITY_LOWEST       THREAD_BASE_PRIORITY_MIN
		#endif
		#ifndef THREAD_PRIORITY_IDLE
		#define THREAD_PRIORITY_IDLE         THREAD_BASE_PRIORITY_IDLE
		#endif

        /************************************/
        /* Defines from winnt.h / winbase.h */
        /************************************/
        #ifndef _WINDEF_ /* a little safer (no warnings) */
			#if defined (_MSC_VER) && (_MSC_VER >= 1200)
				#if defined (STRICT) || !defined (NO_STRICT) || defined (_CVI_)
					#define _CVI_DO_STRICT
				#endif
			#else
				#if defined(STRICT) || defined(_CVI_)
					#define _CVI_DO_STRICT
				#endif
			#endif
			#if defined (_CVI_DO_STRICT)
                struct HINSTANCE__;
                typedef struct HINSTANCE__  *HINSTANCE;
            #else
                typedef void                *HINSTANCE;
            #endif
            typedef int                     BOOL;
			typedef unsigned long           DWORD;
			typedef void                    *LPVOID;
        #endif
        #ifndef _WINNT_ /* a little safer (no warnings) */
            typedef char                    *LPSTR;
			typedef void					*HANDLE;
        #endif
    #endif  /* ndef CVI_EXCLUDE_WINDOWS_TYPES */
#else
/*
 * Begining of NON-WIN32 specific defines
 */

    #define DLLIMPORT
    #define DLLEXPORT
    #define CVIFUNC
    #define CVICALLBACK
    #define CVIFUNC_C
    #define CVIANSI
    #define CVIFUNCPTRVAR
    #define DLLSTDCALL

    #define THREAD_PRIORITY_NORMAL		0
    #define THREAD_PRIORITY_HIGHEST		THREAD_PRIORITY_NORMAL
    #define THREAD_PRIORITY_ABOVE_NORMAL	THREAD_PRIORITY_NORMAL
    #define THREAD_PRIORITY_BELOW_NORMAL	THREAD_PRIORITY_NORMAL
    #define THREAD_PRIORITY_LOWEST		THREAD_PRIORITY_NORMAL
    #define THREAD_PRIORITY_IDLE		THREAD_PRIORITY_NORMAL

#endif /* defined(_NI_mswin32_) (else clause) */

#ifdef _NI_unix_

#if _NI_linux_
#define __int64 long long
#endif

/* The macro _cvi_SYSINCLUDE() defined below is used to include
 * header files from the standard location "/usr/include".
 */
#define _cvi_SYSINCLUDEDIR	/usr/include/

#define _cvi_concat(dir, file)    dir ## file
#define _cvi_CONCAT(dir, file)	  _cvi_concat(dir, file)

#define _cvi_string(x)       # x
#define _cvi_STRING(x)       _cvi_string(x)

#define _cvi_SYSINCLUDE(file)	_cvi_STRING(_cvi_CONCAT(_cvi_SYSINCLUDEDIR, file))

#endif /* def _NI_unix_ */

/*
 * Beginning of defines which are always done
 */

#define _CVI_LIBS_	1501
#if _CVI_ > _CVI_LIBS_
#error _CVI_LIBS is out of date
#endif

#define cviprefix(c) CVI_ ## c

#if !defined(_INTPTR_T_DEFINED) && !defined(__intptr_t_defined)
#ifdef  _NI_mswin64_
typedef __int64             intptr_t;
#else
typedef int                 intptr_t;
#endif
#define _INTPTR_T_DEFINED
#endif

#if !defined(_UINTPTR_T_DEFINED) && !defined(__intptr_t_defined)
#ifdef  _NI_mswin64_
typedef unsigned __int64    uintptr_t;
#else
typedef unsigned int        uintptr_t;
#endif
#define _UINTPTR_T_DEFINED
#endif

#ifndef _SIZE_T_DEFINED
#define _SIZE_T_DEFINED
#ifdef _NI_mswin64_
typedef unsigned __int64    size_t;
#else
typedef unsigned int        size_t;
#endif
#endif

#if !defined(_SSIZE_T_DEFINED) && !defined(__ssize_t_defined)
#define _SSIZE_T_DEFINED
#ifdef _NI_mswin64_
typedef __int64				ssize_t;
#else
typedef int					ssize_t;
#endif
#endif

#define _CVI_PROFILE_LEVEL_DISABLED_        0
#define _CVI_PROFILE_LEVEL_FUNCS_           1
#define _CVI_PROFILE_LEVEL_FUNCS_AND_CALLS_ 2

#ifdef __cplusplus
    }
#endif

#endif /* !defined(_CVIDEF_H) */
