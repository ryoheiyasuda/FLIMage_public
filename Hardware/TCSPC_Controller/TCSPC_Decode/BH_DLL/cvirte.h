/*============================================================================*/
/*                        L a b W i n d o w s / C V I                         */
/*----------------------------------------------------------------------------*/
/*    Copyright (c) National Instruments 1996-2016.   All Rights Reserved.    */
/*----------------------------------------------------------------------------*/
/*                                                                            */
/* Title:       cvirte.h                                                      */
/* Purpose:     Include file for CVI Run-Time Engine DLL.                     */
/*                                                                            */
/*============================================================================*/

#ifndef _CVIRTE_H
#define _CVIRTE_H

#include <cvidef.h>

#ifdef __cplusplus
    extern "C" {
#endif


/*****************************************************************/
/* Pass zero for the hInstance parameter if you are calling this */
/* function from an executable, or the DLL instance if you are   */
/* calling it from a DLL.  You may pass zero for the argv        */
/* parameter if you are not using main(), or you are using       */
/* main with no parameters.  Pass zero for the reserved          */
/* parameter.                                                    */
/*****************************************************************/
int     CVIFUNC     InitCVIRTEEx (void *hInstance, char *argv[],
                                  void *reserved);

#define InitCVIRTE     InitCVIRTEEx

int		CVIFUNC		CVIRTEHasBeenDetached(void);
void    CVIFUNC     CloseCVIRTE (void);

#ifdef __cplusplus
    }
#endif

#endif  /* ndef _CVIRTE_H */
